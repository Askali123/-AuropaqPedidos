using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración del CRUD de Empresa (TASK-006, 05-api.md §11) y de la creación/
// actualización de Sede (TASK-007, 05-api.md §12), a través de la Api real (Controllers +
// Application + Infrastructure + SQL Server de pruebas). Listar()/ListarSedes() ya estaban
// cubiertos por CatalogosFlujoTests; aquí solo se cubre lo nuevo de esta tarea.
// RN-059/060 (punto 8, 2026-09-15): ORGANIZACION_VER/ADMINISTRAR. Administrar Empresa/Sede no
// tiene alcance por empresa (06-seguridad.md §52/§53), así que un único actor "admin" (una
// Empresa+Usuario+Rol bootstrap por número de prueba) puede crear/consultar/actualizar cualquier
// otra Empresa/Sede sin relación con la suya propia.
public sealed class EmpresasSedesCrudFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public EmpresasSedesCrudFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<string> TokenAdminAsync(int numero)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaBootstrap = new AuropaqPedidos.Domain.Entities.Empresa(700_000 + numero, $"Empresa admin bootstrap {numero}");
        db.Empresas.Add(empresaBootstrap);
        await db.SaveChangesAsync();

        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, empresaBootstrap.Id, "ORGANIZACION_VER", "ORGANIZACION_ADMINISTRAR");
    }

    private static HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    [Fact]
    public async Task Crear_empresa_responde_201_y_queda_activa()
    {
        var token = await TokenAdminAsync(201);
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/empresas", token);
        solicitud.Content = JsonContent.Create(new { nombre = "AUROTECH", nit = "900000000-1" });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EmpresaDto>>();
        Assert.Equal("AUROTECH", cuerpo!.Data.Nombre);
        Assert.True(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Obtener_empresa_devuelve_la_empresa_creada()
    {
        var token = await TokenAdminAsync(202);
        var creada = await CrearEmpresaAsync(token, "FAVIPAQ");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/empresas/{creada.Id}", token));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EmpresaDto>>();
        Assert.Equal(creada.Id, cuerpo!.Data.Id);
    }

    [Fact]
    public async Task Obtener_empresa_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync(203);

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/empresas/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Actualizar_empresa_cambia_nombre_nit_y_puede_desactivarla()
    {
        var token = await TokenAdminAsync(204);
        var creada = await CrearEmpresaAsync(token, "COURIERBOX");

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/empresas/{creada.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "COURIERBOX ÁLAMOS",
            nit = "900222222-3",
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EmpresaDto>>();
        Assert.Equal("COURIERBOX ÁLAMOS", cuerpo!.Data.Nombre);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Actualizar_empresa_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync(205);
        var solicitud = ConToken(HttpMethod.Put, "/api/v1/empresas/999999", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "X",
            nit = (string?)null,
            activo = true,
        });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_sede_responde_201_y_queda_asociada_a_la_empresa()
    {
        var token = await TokenAdminAsync(206);
        var empresa = await CrearEmpresaAsync(token, "AUROTECH");

        var solicitud = ConToken(HttpMethod.Post, $"/api/v1/empresas/{empresa.Id}/sedes", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Sede Bogotá",
            direccion = "Calle 1 # 2-3",
            ciudad = "Bogotá",
            departamento = "Bogotá D.C.",
            telefono = "6011234567",
            contacto = "Recepción",
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<SedeDto>>();
        Assert.Equal("Sede Bogotá", cuerpo!.Data.Nombre);
        Assert.Equal("Bogotá", cuerpo.Data.Ciudad);
        Assert.True(cuerpo.Data.Activo);

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/empresas/{empresa.Id}/sedes", token));
        var cuerpoListado = await listado.Content.ReadFromJsonAsync<Envoltorio<List<SedeDto>>>();
        Assert.Contains(cuerpoListado!.Data, s => s.Id == cuerpo.Data.Id);
    }

    [Fact]
    public async Task Crear_sede_para_empresa_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync(207);
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/empresas/999999/sedes", token);
        solicitud.Content = JsonContent.Create(new { nombre = "Sede X" });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_sede_para_empresa_inactiva_devuelve_422()
    {
        var token = await TokenAdminAsync(208);
        var empresa = await CrearEmpresaAsync(token, "EMPRESA INACTIVA");
        var desactivar = ConToken(HttpMethod.Put, $"/api/v1/empresas/{empresa.Id}", token);
        desactivar.Content = JsonContent.Create(new { nombre = empresa.Nombre, nit = (string?)null, activo = false });
        await _cliente.SendAsync(desactivar);

        var solicitud = ConToken(HttpMethod.Post, $"/api/v1/empresas/{empresa.Id}/sedes", token);
        solicitud.Content = JsonContent.Create(new { nombre = "Sede X" });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Actualizar_sede_cambia_sus_datos_y_puede_desactivarla()
    {
        var token = await TokenAdminAsync(209);
        var empresa = await CrearEmpresaAsync(token, "PROVEEDORA DE PRUEBA");
        var crearSede = ConToken(HttpMethod.Post, $"/api/v1/empresas/{empresa.Id}/sedes", token);
        crearSede.Content = JsonContent.Create(new { nombre = "Sede Original" });
        var sedeCreada = await _cliente.SendAsync(crearSede);
        var sede = (await sedeCreada.Content.ReadFromJsonAsync<Envoltorio<SedeDto>>())!.Data;

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/sedes/{sede.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Sede Renombrada",
            direccion = "Nueva dirección",
            ciudad = "Medellín",
            departamento = "Antioquia",
            telefono = "6041234567",
            contacto = "Portería",
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<SedeDto>>();
        Assert.Equal("Sede Renombrada", cuerpo!.Data.Nombre);
        Assert.Equal("Medellín", cuerpo.Data.Ciudad);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Actualizar_sede_inexistente_devuelve_404()
    {
        var token = await TokenAdminAsync(210);
        var solicitud = ConToken(HttpMethod.Put, "/api/v1/sedes/999999", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "X",
            direccion = (string?)null,
            ciudad = (string?)null,
            departamento = (string?)null,
            telefono = (string?)null,
            contacto = (string?)null,
            activo = true,
        });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    private async Task<EmpresaDto> CrearEmpresaAsync(string token, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/empresas", token);
        solicitud.Content = JsonContent.Create(new { nombre, nit = (string?)null });
        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EmpresaDto>>();
        return cuerpo!.Data;
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record EmpresaDto(int Id, string Nombre, bool Activo);

    private sealed record SedeDto(int Id, string Nombre, string? Direccion, string? Ciudad, string? Departamento, string? Telefono, string? Contacto, bool Activo);
}
