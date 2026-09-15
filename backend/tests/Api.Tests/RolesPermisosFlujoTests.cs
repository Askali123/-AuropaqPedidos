using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de Rol/Permiso/UsuarioRol/UsuarioSede/RolPermiso (TASK-009/010/011/012/
// 013, 05-api.md §59-§63, ADR-061), a través de la Api real (Controllers + Application +
// Infrastructure + SQL Server de pruebas). RN-059/060 (punto 8, 2026-09-15): SEGURIDAD_VER/
// ADMINISTRAR, sin alcance por empresa — un único actor bootstrap alcanza para toda la clase. El
// actor bootstrap y los usuarios de prueba son siempre personas distintas, así que nunca chocan
// con la prevención de auto-escalamiento (RN-060 punto 7).
public sealed class RolesPermisosFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const int NumeroBootstrap = 90_030;

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public RolesPermisosFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<Escenario> NuevoEscenarioAsync(int numero)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await Escenario.CrearAsync(db, numero);
    }

    private async Task<string> TokenAdminAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + NumeroBootstrap;
        if (await db.Empresas.FindAsync(empresaId) is null)
        {
            db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap seguridad"));
            await db.SaveChangesAsync();
        }

        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, NumeroBootstrap, empresaId, "SEGURIDAD_VER", "SEGURIDAD_ADMINISTRAR");
    }

    private HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    private async Task<int> CrearUsuarioAsync(string token, int empresaId, string sufijo)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/usuarios", token);
        solicitud.Content = JsonContent.Create(new
        {
            empresaId,
            nombre = "Usuario de prueba",
            correo = $"rolespermisos.{sufijo}@auropaq.com",
            password = "password123",
        });
        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<UsuarioDto>>();
        return cuerpo!.Data.Id;
    }

    private async Task<RolDto> CrearRolAsync(string token, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/roles", token);
        solicitud.Content = JsonContent.Create(new { nombre });
        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<RolDto>>();
        return cuerpo!.Data;
    }

    private async Task<PermisoDto> CrearPermisoAsync(string token, string codigo)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/permisos", token);
        solicitud.Content = JsonContent.Create(new { codigo, nombre = codigo });
        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PermisoDto>>();
        return cuerpo!.Data;
    }

    [Fact]
    public async Task Crear_rol_responde_201_y_aparece_en_el_listado()
    {
        var token = await TokenAdminAsync();
        var rol = await CrearRolAsync(token, "Solicitante 301");

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/roles", token));
        var cuerpo = await listado.Content.ReadFromJsonAsync<Envoltorio<List<RolDto>>>();
        Assert.Contains(cuerpo!.Data, r => r.Id == rol.Id && r.Activo);
    }

    [Fact]
    public async Task Crear_permiso_responde_201_y_aparece_en_el_listado()
    {
        var token = await TokenAdminAsync();
        var permiso = await CrearPermisoAsync(token, "REQUISICION_CREAR_302");

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/permisos", token));
        var cuerpo = await listado.Content.ReadFromJsonAsync<Envoltorio<List<PermisoDto>>>();
        Assert.Contains(cuerpo!.Data, p => p.Id == permiso.Id);
    }

    [Fact]
    public async Task Crear_permiso_con_codigo_duplicado_devuelve_422()
    {
        var token = await TokenAdminAsync();
        await CrearPermisoAsync(token, "REQUISICION_VER_303");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/permisos", token);
        solicitud.Content = JsonContent.Create(new { codigo = "REQUISICION_VER_303", nombre = "Otro nombre" });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Asignar_rol_a_usuario_responde_201_y_aparece_en_sus_roles()
    {
        var token = await TokenAdminAsync();
        var escenario = await NuevoEscenarioAsync(304);
        var usuarioId = await CrearUsuarioAsync(token, escenario.EmpresaId, "304");
        var rol = await CrearRolAsync(token, "Revisor 304");

        var asignar = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioId}/roles", token);
        asignar.Content = JsonContent.Create(new { rolId = rol.Id });
        var respuesta = await _cliente.SendAsync(asignar);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var roles = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/usuarios/{usuarioId}/roles", token));
        var cuerpo = await roles.Content.ReadFromJsonAsync<Envoltorio<List<RolDto>>>();
        Assert.Contains(cuerpo!.Data, r => r.Id == rol.Id);
    }

    [Fact]
    public async Task Asignar_el_mismo_rol_dos_veces_devuelve_422()
    {
        var token = await TokenAdminAsync();
        var escenario = await NuevoEscenarioAsync(305);
        var usuarioId = await CrearUsuarioAsync(token, escenario.EmpresaId, "305");
        var rol = await CrearRolAsync(token, "Administrador 305");
        var primeraAsignacion = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioId}/roles", token);
        primeraAsignacion.Content = JsonContent.Create(new { rolId = rol.Id });
        await _cliente.SendAsync(primeraAsignacion);

        var segundaAsignacion = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioId}/roles", token);
        segundaAsignacion.Content = JsonContent.Create(new { rolId = rol.Id });
        var respuesta = await _cliente.SendAsync(segundaAsignacion);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Asignar_rol_a_usuario_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync();
        var rol = await CrearRolAsync(token, "Compras 306");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/usuarios/999999/roles", token);
        solicitud.Content = JsonContent.Create(new { rolId = rol.Id });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_roles_de_usuario_sin_roles_devuelve_lista_vacia()
    {
        var token = await TokenAdminAsync();
        var escenario = await NuevoEscenarioAsync(307);
        var usuarioId = await CrearUsuarioAsync(token, escenario.EmpresaId, "307");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/usuarios/{usuarioId}/roles", token));
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<RolDto>>>();

        Assert.Empty(cuerpo!.Data);
    }

    [Fact]
    public async Task Asignar_sede_a_usuario_responde_201_y_aparece_en_sus_sedes_autorizadas()
    {
        var token = await TokenAdminAsync();
        var escenario = await NuevoEscenarioAsync(308);
        var usuarioId = await CrearUsuarioAsync(token, escenario.EmpresaId, "308");

        var asignar = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioId}/sedes", token);
        asignar.Content = JsonContent.Create(new { sedeId = escenario.SedeId });
        var respuesta = await _cliente.SendAsync(asignar);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var sedes = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/usuarios/{usuarioId}/sedes", token));
        var cuerpo = await sedes.Content.ReadFromJsonAsync<Envoltorio<List<SedeDto>>>();
        Assert.Contains(cuerpo!.Data, s => s.Id == escenario.SedeId);
    }

    [Fact]
    public async Task Asignar_sede_de_otra_empresa_devuelve_422()
    {
        var token = await TokenAdminAsync();
        var escenarioUsuario = await NuevoEscenarioAsync(309);
        var escenarioSede = await NuevoEscenarioAsync(310);
        var usuarioId = await CrearUsuarioAsync(token, escenarioUsuario.EmpresaId, "309");

        var solicitud = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioId}/sedes", token);
        solicitud.Content = JsonContent.Create(new { sedeId = escenarioSede.SedeId });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Asignar_permiso_a_rol_responde_201_y_aparece_en_sus_permisos()
    {
        var token = await TokenAdminAsync();
        var rol = await CrearRolAsync(token, "Aprobador 311");
        var permiso = await CrearPermisoAsync(token, "REQUISICION_APROBAR_311");

        var asignar = ConToken(HttpMethod.Post, $"/api/v1/roles/{rol.Id}/permisos", token);
        asignar.Content = JsonContent.Create(new { permisoId = permiso.Id });
        var respuesta = await _cliente.SendAsync(asignar);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var permisos = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/roles/{rol.Id}/permisos", token));
        var cuerpo = await permisos.Content.ReadFromJsonAsync<Envoltorio<List<PermisoDto>>>();
        Assert.Contains(cuerpo!.Data, p => p.Id == permiso.Id);
    }

    [Fact]
    public async Task Asignar_permiso_a_rol_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync();
        var permiso = await CrearPermisoAsync(token, "REQUISICION_DEVOLVER_312");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/roles/999999/permisos", token);
        solicitud.Content = JsonContent.Create(new { permisoId = permiso.Id });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    // RN-060 punto 7 (06-seguridad.md §62): prevención de escalamiento de privilegios.
    [Fact]
    public async Task Asignar_rol_a_si_mismo_devuelve_422()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + 313;
        db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap 313"));
        await db.SaveChangesAsync();
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 313, empresaId, "SEGURIDAD_ADMINISTRAR");
        // El propio actor autenticado (creado por AutorizacionHelper con id = baseId+1) es el
        // usuario destino: intenta asignarse un rol a sí mismo.
        var usuarioActorId = 600_000 + (313 * 100) + 1;
        var rolToken = await TokenAdminAsync();
        var rol = await CrearRolAsync(rolToken, "Autoescalamiento 313");

        var solicitud = ConToken(HttpMethod.Post, $"/api/v1/usuarios/{usuarioActorId}/roles", token);
        solicitud.Content = JsonContent.Create(new { rolId = rol.Id });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record UsuarioDto(int Id, int EmpresaId, string Nombre);

    private sealed record RolDto(int Id, string Nombre, string? Descripcion, bool Activo);

    private sealed record PermisoDto(int Id, string Codigo, string Nombre, string? Descripcion);

    private sealed record SedeDto(int Id, string Nombre, bool Activo);
}
