using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración del endpoint de creación de Periodo (docs/05-api.md §16.3), a través
// de la Api real (Controllers + Application + Infrastructure + SQL Server de pruebas). Permite
// verificar en vivo (fuera de la suite también) el flujo completo de estados de Requisición sin
// depender de un Periodo cuya ventana de solicitud ya venció.
// RN-059/060 (punto 8, 2026-09-15): PERIODO_VER/CREAR, sin alcance por empresa.
public sealed class PeriodosFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const int NumeroBootstrap = 90_016;

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public PeriodosFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<string> TokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + NumeroBootstrap;
        if (await db.Empresas.FindAsync(empresaId) is null)
        {
            db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap periodos"));
            await db.SaveChangesAsync();
        }

        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, NumeroBootstrap, empresaId, "PERIODO_VER", "PERIODO_CREAR");
    }

    private HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    [Fact]
    public async Task Crear_periodo_responde_201_con_estado_ABIERTO()
    {
        var token = await TokenAsync();
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/periodos", token);
        solicitud.Content = JsonContent.Create(new
        {
            anio = 2030,
            mes = 1,
            fechaInicio = new DateTime(2030, 1, 1),
            fechaFin = new DateTime(2030, 1, 31),
            fechaInicioSolicitud = new DateTime(2030, 1, 1),
            fechaFinSolicitud = new DateTime(2030, 1, 5),
        });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PeriodoDto>>();
        Assert.Equal("ABIERTO", cuerpo!.Data.Estado);
        Assert.Equal(2030, cuerpo.Data.Anio);
        Assert.Equal(1, cuerpo.Data.Mes);
    }

    [Fact]
    public async Task Obtener_periodo_devuelve_el_periodo_creado()
    {
        var token = await TokenAsync();
        var crear = ConToken(HttpMethod.Post, "/api/v1/periodos", token);
        crear.Content = JsonContent.Create(new
        {
            anio = 2031,
            mes = 1,
            fechaInicio = new DateTime(2031, 1, 1),
            fechaFin = new DateTime(2031, 1, 31),
            fechaInicioSolicitud = new DateTime(2031, 1, 1),
            fechaFinSolicitud = new DateTime(2031, 1, 5),
        });
        var creado = await _cliente.SendAsync(crear);
        var periodo = (await creado.Content.ReadFromJsonAsync<Envoltorio<PeriodoDto>>())!.Data;

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/periodos/{periodo.Id}", token));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PeriodoDto>>();
        Assert.Equal(periodo.Id, cuerpo!.Data.Id);
    }

    [Fact]
    public async Task Obtener_periodo_inexistente_devuelve_404()
    {
        var token = await TokenAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/periodos/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_periodo_duplicado_para_mismo_anio_y_mes_devuelve_422()
    {
        var token = await TokenAsync();
        var body = new
        {
            anio = 2030,
            mes = 2,
            fechaInicio = new DateTime(2030, 2, 1),
            fechaFin = new DateTime(2030, 2, 28),
            fechaInicioSolicitud = new DateTime(2030, 2, 1),
            fechaFinSolicitud = new DateTime(2030, 2, 5),
        };
        var primero = ConToken(HttpMethod.Post, "/api/v1/periodos", token);
        primero.Content = JsonContent.Create(body);
        await _cliente.SendAsync(primero);

        var segundo = ConToken(HttpMethod.Post, "/api/v1/periodos", token);
        segundo.Content = JsonContent.Create(body);
        var respuesta = await _cliente.SendAsync(segundo);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record PeriodoDto(int Id, int Anio, int Mes, string Estado);
}
