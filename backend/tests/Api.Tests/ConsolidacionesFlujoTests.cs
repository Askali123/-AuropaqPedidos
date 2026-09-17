using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de ConsolidacionesController (P2-1, docs/2026-09-17-tareas.md) a través
// de la Api real (Controllers + Application + Infrastructure + SQL Server), según
// 03-arquitectura.md §42. Cubre únicamente Crear (el único caso de uso real, TASK-036) — mismo
// criterio de minimalismo que FacturasFlujoTests/PedidosProveedorFlujoTests.
public sealed class ConsolidacionesFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public ConsolidacionesFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private static HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    // Crea, envía y aprueba una Requisición completa por HTTP (mismo patrón que
    // FlujoIntegradoA2B1B4Tests.CrearYAprobarRequisicionAsync), devolviendo también el token
    // usado para poder reutilizar sus permisos si hiciera falta.
    private async Task<(Escenario escenario, int requisicionId)> CrearYAprobarRequisicionAsync(int numero, int cantidad)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, numero);

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, escenario.EmpresaId,
            "REQUISICION_CREAR", "REQUISICION_ENVIAR", "REQUISICION_APROBAR");

        var crear = ConToken(HttpMethod.Post, "/api/v1/requisiciones", token);
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });
        var respuestaCrear = await _cliente.SendAsync(crear);
        var requisicionId = (await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Id;

        var agregarDetalle = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles", token);
        agregarDetalle.Content = JsonContent.Create(new { productoId = escenario.ProductoId, cantidadSolicitada = cantidad, observacion = (string?)null });
        var respuestaDetalle = await _cliente.SendAsync(agregarDetalle);
        var detalleId = (await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Detalles[0].Id;

        var distribuir = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones", token);
        distribuir.Content = JsonContent.Create(new { sedeId = escenario.SedeId, cantidad });
        await _cliente.SendAsync(distribuir);

        await _cliente.SendAsync(ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar", token));
        await _cliente.SendAsync(ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision", token));

        var aprobar = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/aprobar", token);
        aprobar.Content = JsonContent.Create(new { observacion = (string?)null });
        await _cliente.SendAsync(aprobar);

        return (escenario, requisicionId);
    }

    [Fact]
    public async Task Crear_consolidacion_agrupa_la_requisicion_aprobada_del_periodo()
    {
        var (escenario, _) = await CrearYAprobarRequisicionAsync(numero: 1, cantidad: 30);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 1001, escenario.EmpresaId, "PEDIDO_CONSOLIDAR");

        var crear = ConToken(HttpMethod.Post, "/api/v1/consolidaciones", token);
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId, estado = "GENERADA" });
        var respuesta = await _cliente.SendAsync(crear);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var consolidacion = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ConsolidacionDto>>();
        Assert.Equal(escenario.PeriodoId, consolidacion!.Data.PeriodoId);
        Assert.Equal("GENERADA", consolidacion.Data.Estado);
        var detalle = Assert.Single(consolidacion.Data.Detalles);
        Assert.Equal(30, detalle.CantidadNecesaria);
    }

    [Fact]
    public async Task Crear_consolidacion_con_periodo_inexistente_devuelve_404()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 2);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 2, escenario.EmpresaId, "PEDIDO_CONSOLIDAR");

        var crear = ConToken(HttpMethod.Post, "/api/v1/consolidaciones", token);
        crear.Content = JsonContent.Create(new { periodoId = 999999, estado = "GENERADA" });
        var respuesta = await _cliente.SendAsync(crear);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_consolidacion_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/consolidaciones", new { periodoId = 1, estado = "GENERADA" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_consolidacion_con_jwt_valido_sin_permiso_devuelve_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 3);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 3, escenario.EmpresaId); // sin PEDIDO_CONSOLIDAR

        var crear = ConToken(HttpMethod.Post, "/api/v1/consolidaciones", token);
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId, estado = "GENERADA" });
        var respuesta = await _cliente.SendAsync(crear);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_consolidaciones_filtradas_por_periodo_devuelve_solo_las_del_periodo()
    {
        var (escenario, _) = await CrearYAprobarRequisicionAsync(numero: 4, cantidad: 15);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 1004, escenario.EmpresaId, "PEDIDO_CONSOLIDAR", "PEDIDO_VER");

        var crear = ConToken(HttpMethod.Post, "/api/v1/consolidaciones", token);
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId, estado = "GENERADA" });
        var creada = await (await _cliente.SendAsync(crear)).Content.ReadFromJsonAsync<Envoltorio<ConsolidacionDto>>();

        var respuesta = await _cliente.SendAsync(
            ConToken(HttpMethod.Get, $"/api/v1/consolidaciones?periodoId={escenario.PeriodoId}", token));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var lista = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<ConsolidacionDto>>>();
        Assert.Contains(lista!.Data, c => c.Id == creada!.Data.Id);
    }

    [Fact]
    public async Task Obtener_consolidacion_inexistente_devuelve_404()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 5);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 5, escenario.EmpresaId, "PEDIDO_VER");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/consolidaciones/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    // I4-1 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): guardia de regresión para los
    // GET agregados en I1-1 — mismo patrón ya usado arriba para Crear (401/403 sin token/permiso).
    [Fact]
    public async Task Listar_consolidaciones_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/consolidaciones");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_consolidacion_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/consolidaciones/1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_y_obtener_consolidacion_con_jwt_valido_sin_permiso_devuelven_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 6);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 6, escenario.EmpresaId); // sin PEDIDO_VER

        var listar = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/consolidaciones", token));
        var obtener = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/consolidaciones/1", token));

        Assert.Equal(HttpStatusCode.Forbidden, listar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, obtener.StatusCode);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record RequisicionDto(int Id, IReadOnlyList<DetalleDto> Detalles);

    private sealed record DetalleDto(int Id);

    private sealed record ConsolidacionDto(int Id, int PeriodoId, string Estado, IReadOnlyList<DetalleConsolidacionDto> Detalles);

    private sealed record DetalleConsolidacionDto(int Id, int ProductoId, int CantidadNecesaria);
}
