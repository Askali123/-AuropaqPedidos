using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de los endpoints de solo lectura para Empresas/Sedes/Productos/Periodos
// (TASK: habilitar las consultas necesarias para el Frontend de Requisiciones, docs/05-api.md
// §54.6), a través de la Api real (Controllers + Application + Infrastructure + SQL Server).
// RN-059/060 (punto 8, 2026-09-15): ahora exigen JWT + ORGANIZACION_VER/PRODUCTO_VER/PERIODO_VER.
public sealed class CatalogosFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public CatalogosFlujoTests(ApiWebApplicationFactory factory)
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

    private async Task<string> TokenAsync(int numero, int empresaId, params string[] permisos)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, numero, empresaId, permisos);
    }

    private async Task<HttpResponseMessage> GetAutenticadoAsync(string url, string token)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _cliente.SendAsync(solicitud);
    }

    [Fact]
    public async Task Listar_empresas_incluye_la_empresa_creada()
    {
        var escenario = await NuevoEscenarioAsync(101);
        var token = await TokenAsync(101, escenario.EmpresaId, "ORGANIZACION_VER");

        var respuesta = await GetAutenticadoAsync("/api/v1/empresas", token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<EmpresaDto>>>();
        Assert.Contains(cuerpo!.Data, e => e.Id == escenario.EmpresaId && e.Activo);
    }

    [Fact]
    public async Task Listar_sedes_de_una_empresa_devuelve_solo_las_suyas()
    {
        var escenario1 = await NuevoEscenarioAsync(102);
        var escenario2 = await NuevoEscenarioAsync(103);
        var token = await TokenAsync(102, escenario1.EmpresaId, "ORGANIZACION_VER");

        var respuesta = await GetAutenticadoAsync($"/api/v1/empresas/{escenario1.EmpresaId}/sedes", token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<SedeDto>>>();
        Assert.Contains(cuerpo!.Data, s => s.Id == escenario1.SedeId);
        Assert.DoesNotContain(cuerpo.Data, s => s.Id == escenario2.SedeId);
    }

    [Fact]
    public async Task Listar_sedes_de_una_empresa_sin_sedes_devuelve_lista_vacia()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaSinSedes = new Empresa(9001, "Empresa sin sedes");
        db.Empresas.Add(empresaSinSedes);
        await db.SaveChangesAsync();
        var token = await TokenAsync(9001, empresaSinSedes.Id, "ORGANIZACION_VER");

        var respuesta = await GetAutenticadoAsync($"/api/v1/empresas/{empresaSinSedes.Id}/sedes", token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<SedeDto>>>();
        Assert.Empty(cuerpo!.Data);
    }

    [Fact]
    public async Task Listar_sedes_de_una_empresa_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(106);
        var token = await TokenAsync(106, escenario.EmpresaId, "ORGANIZACION_VER");

        var respuesta = await GetAutenticadoAsync("/api/v1/empresas/999999/sedes", token);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Listar_productos_incluye_el_producto_creado_con_su_unidad_de_medida()
    {
        var escenario = await NuevoEscenarioAsync(104);
        var token = await TokenAsync(104, escenario.EmpresaId, "PRODUCTO_VER");

        var respuesta = await GetAutenticadoAsync("/api/v1/productos", token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<ProductoDto>>>();
        var producto = Assert.Single(cuerpo!.Data, p => p.Id == escenario.ProductoId);
        Assert.Equal("UNIDAD", producto.UnidadMedidaCodigo);
    }

    [Fact]
    public async Task Listar_periodos_incluye_el_periodo_creado()
    {
        var escenario = await NuevoEscenarioAsync(105);
        var token = await TokenAsync(105, escenario.EmpresaId, "PERIODO_VER");

        var respuesta = await GetAutenticadoAsync("/api/v1/periodos", token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<PeriodoDto>>>();
        Assert.Contains(cuerpo!.Data, p => p.Id == escenario.PeriodoId);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record EmpresaDto(int Id, string Nombre, bool Activo);

    private sealed record SedeDto(int Id, string Nombre, bool Activo);

    private sealed record ProductoDto(int Id, string Nombre, string? CodigoInterno, string UnidadMedidaCodigo, string UnidadMedidaNombre, bool Activo);

    private sealed record PeriodoDto(int Id, int Anio, int Mes, string Estado);
}
