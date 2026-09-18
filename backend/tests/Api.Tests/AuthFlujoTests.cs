using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// TASK-015 (POST /api/v1/auth/login), a través de la Api real (Controllers + Application +
// Infrastructure + SQL Server de pruebas). Crea el usuario primero por HTTP real
// (POST /api/v1/usuarios), igual que lo haría el flujo real, en vez de insertarlo directo en BD.
//
// RN-059/060 (punto 8, 2026-09-15): POST /usuarios ahora exige JWT + SEGURIDAD_ADMINISTRAR — el
// usuario "administrador" que lo crea es un actor distinto del usuario "Martha" bajo prueba de
// login, así que usa su propio "numero" (+90_000) para no mezclar permisos entre ambos.
public sealed class AuthFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public AuthFlujoTests(ApiWebApplicationFactory factory)
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

    private async Task<string> CrearUsuarioAsync(int numero, int empresaId, string correo, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var tokenAdmin = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero + 90_000, empresaId, "SEGURIDAD_ADMINISTRAR");

        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/usuarios");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);
        solicitud.Content = JsonContent.Create(new
        {
            empresaId,
            nombre = "Martha",
            correo,
            password,
        });
        var respuesta = await _cliente.SendAsync(solicitud);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return correo;
    }

    [Fact]
    public async Task Login_valido_responde_200_con_token()
    {
        var escenario = await NuevoEscenarioAsync(301);
        var correo = await CrearUsuarioAsync(301, escenario.EmpresaId, "martha.301@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<LoginDto>>();
        Assert.False(string.IsNullOrWhiteSpace(cuerpo!.Data.Token));
        Assert.Equal(correo, cuerpo.Data.Usuario.Correo);
    }

    [Fact]
    public async Task Login_no_expone_password_ni_hash_en_la_respuesta()
    {
        var escenario = await NuevoEscenarioAsync(302);
        var correo = await CrearUsuarioAsync(302, escenario.EmpresaId, "martha.302@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password123", cuerpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_con_password_incorrecta_devuelve_401()
    {
        var escenario = await NuevoEscenarioAsync(303);
        var correo = await CrearUsuarioAsync(303, escenario.EmpresaId, "martha.303@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "otro-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    [Fact]
    public async Task Login_con_correo_inexistente_devuelve_401()
    {
        var respuesta = await _cliente.PostAsJsonAsync(
            "/api/v1/auth/login", new { correo = "no-existe@auropaq.com", password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    [Fact]
    public async Task Login_con_usuario_inactivo_devuelve_401_con_el_mismo_codigo()
    {
        var escenario = await NuevoEscenarioAsync(304);
        var correo = await CrearUsuarioAsync(304, escenario.EmpresaId, "martha.304@auropaq.com", "password123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var usuario = db.Usuarios.Single(u => u.Correo == correo);
            usuario.Desactivar(DateTime.UtcNow);
            await db.SaveChangesAsync();
        }

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    // TASK-101 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, Decisión A): autoconsulta de
    // permisos, base para que el Frontend pueda filtrar navegación sin depender de SEGURIDAD_VER.
    [Fact]
    public async Task MisPermisos_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/auth/mis-permisos");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task MisPermisos_devuelve_exactamente_los_permisos_reales_del_usuario()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 305);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 305, escenario.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER");

        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/mis-permisos");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<PermisoDto>>>();
        var codigos = cuerpo!.Data.Select(p => p.Codigo).ToList();
        Assert.Equal(2, codigos.Count);
        Assert.Contains("REQUISICION_CREAR", codigos);
        Assert.Contains("REQUISICION_VER", codigos);
    }

    [Fact]
    public async Task MisPermisos_de_un_usuario_sin_ningun_rol_devuelve_lista_vacia()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 306);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, 306, escenario.EmpresaId);

        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/mis-permisos");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<PermisoDto>>>();
        Assert.Empty(cuerpo!.Data);
    }

    private sealed record PermisoDto(int Id, string Codigo, string Nombre, string? Descripcion);

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record LoginDto(string Token, DateTime FechaExpiracion, UsuarioDto Usuario);

    private sealed record UsuarioDto(
        int Id, int EmpresaId, string Nombre, string? Apellido, string Correo, bool Activo,
        DateTime FechaCreacion, DateTime FechaActualizacion);
}
