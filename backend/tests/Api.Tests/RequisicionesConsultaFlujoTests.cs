using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de las consultas de Requisición (TASK-031/032, 05-api.md §17.1/§17.2/
// §24.1): GET /requisiciones (alcance por empresa), GET /requisiciones/{id} (RN-059: TASK-032 no
// le había puesto alcance por error — corregido en el punto 8, ver ADR-062/06-seguridad.md §52) y
// GET /requisiciones/pendientes-revision (alcance por empresa).
//
// RN-059/060 (punto 8, 2026-09-15): la Empresa de "Listar"/"ListarPendientesDeRevision" ahora se
// deriva del Usuario autenticado (JWT), no de un header — `X-Usuario-Id`/`X-Empresa-Id` ya no
// existen en este Controller.
public sealed class RequisicionesConsultaFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public RequisicionesConsultaFlujoTests(ApiWebApplicationFactory factory)
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

    private static HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    private async Task<int> CrearBorradorAsync(Escenario escenario, string token)
    {
        var crear = ConToken(HttpMethod.Post, "/api/v1/requisiciones", token);
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });
        var respuesta = await _cliente.SendAsync(crear);
        var requisicion = await respuesta.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        return requisicion!.Data.Id;
    }

    private async Task<int> CrearEnRevisionAsync(Escenario escenario, string token)
    {
        var requisicionId = await CrearBorradorAsync(escenario, token);

        var agregarDetalle = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles", token);
        agregarDetalle.Content = JsonContent.Create(new { productoId = escenario.ProductoId, cantidadSolicitada = 10, observacion = (string?)null });
        var respuestaDetalle = await _cliente.SendAsync(agregarDetalle);
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionConDetallesDto>>();
        var detalleId = conDetalle!.Data.Detalles[0].Id;

        var distribuir = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones", token);
        distribuir.Content = JsonContent.Create(new { sedeId = escenario.SedeId, cantidad = 10 });
        await _cliente.SendAsync(distribuir);

        var enviar = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar", token);
        var respuestaEnviar = await _cliente.SendAsync(enviar);
        Assert.Equal(HttpStatusCode.OK, respuestaEnviar.StatusCode);

        var iniciarRevision = ConToken(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision", token);
        var respuestaRevision = await _cliente.SendAsync(iniciarRevision);
        Assert.Equal(HttpStatusCode.OK, respuestaRevision.StatusCode);

        return requisicionId;
    }

    [Fact]
    public async Task Obtener_requisicion_devuelve_el_detalle_completo()
    {
        var escenario = await NuevoEscenarioAsync(601);
        var token = await TokenAsync(601, escenario.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER");
        var requisicionId = await CrearBorradorAsync(escenario, token);

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/requisiciones/{requisicionId}", token));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal(requisicionId, cuerpo!.Data.Id);
        Assert.Equal("Borrador", cuerpo.Data.Estado);
    }

    [Fact]
    public async Task Obtener_requisicion_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(606);
        var token = await TokenAsync(606, escenario.EmpresaId, "REQUISICION_VER");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/requisiciones/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/requisiciones");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_sin_permiso_REQUISICION_VER_devuelve_403()
    {
        var escenario = await NuevoEscenarioAsync(607);
        var token = await TokenAsync(607, escenario.EmpresaId, "PERIODO_VER");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/requisiciones", token));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_solo_incluye_las_requisiciones_de_la_empresa_del_usuario_autenticado()
    {
        var escenarioA = await NuevoEscenarioAsync(602);
        var escenarioB = await NuevoEscenarioAsync(603);
        var tokenA = await TokenAsync(602, escenarioA.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER");
        var tokenB = await TokenAsync(603, escenarioB.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER");
        var idA = await CrearBorradorAsync(escenarioA, tokenA);
        await CrearBorradorAsync(escenarioB, tokenB);

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/requisiciones", tokenA));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<RequisicionDto>>>();
        Assert.Contains(cuerpo!.Data, r => r.Id == idA);
        Assert.All(cuerpo.Data, r => Assert.Equal(escenarioA.EmpresaId, r.EmpresaId));
    }

    [Fact]
    public async Task Listar_pendientes_de_revision_solo_incluye_las_en_revision_de_la_empresa()
    {
        var escenarioA = await NuevoEscenarioAsync(604);
        var escenarioB = await NuevoEscenarioAsync(605);
        var tokenA = await TokenAsync(604, escenarioA.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER", "REQUISICION_ENVIAR", "REQUISICION_APROBAR");
        var tokenB = await TokenAsync(605, escenarioB.EmpresaId, "REQUISICION_CREAR", "REQUISICION_VER", "REQUISICION_ENVIAR", "REQUISICION_APROBAR");

        var enRevisionA = await CrearEnRevisionAsync(escenarioA, tokenA);
        await CrearBorradorAsync(escenarioA, tokenA);
        await CrearEnRevisionAsync(escenarioB, tokenB);

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/requisiciones/pendientes-revision", tokenA));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<RequisicionDto>>>();
        var unica = Assert.Single(cuerpo!.Data);
        Assert.Equal(enRevisionA, unica.Id);
        Assert.Equal("EnRevision", unica.Estado);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record RequisicionDto(int Id, int EmpresaId, int PeriodoId, string Estado);

    private sealed record RequisicionConDetallesDto(int Id, List<DetalleDto> Detalles);

    private sealed record DetalleDto(int Id);
}
