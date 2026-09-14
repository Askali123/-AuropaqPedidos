using System.Net;
using System.Net.Http.Json;

namespace Api.Tests;

// Pruebas de integración del endpoint de creación de Periodo (docs/05-api.md §16.3), a través
// de la Api real (Controllers + Application + Infrastructure + SQL Server de pruebas). Permite
// verificar en vivo (fuera de la suite también) el flujo completo de estados de Requisición sin
// depender de un Periodo cuya ventana de solicitud ya venció.
public sealed class PeriodosFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _cliente;

    public PeriodosFlujoTests(ApiWebApplicationFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Crear_periodo_responde_201_con_estado_ABIERTO()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/periodos", new
        {
            anio = 2030,
            mes = 1,
            fechaInicio = new DateTime(2030, 1, 1),
            fechaFin = new DateTime(2030, 1, 31),
            fechaInicioSolicitud = new DateTime(2030, 1, 1),
            fechaFinSolicitud = new DateTime(2030, 1, 5),
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PeriodoDto>>();
        Assert.Equal("ABIERTO", cuerpo!.Data.Estado);
        Assert.Equal(2030, cuerpo.Data.Anio);
        Assert.Equal(1, cuerpo.Data.Mes);
    }

    [Fact]
    public async Task Crear_periodo_duplicado_para_mismo_anio_y_mes_devuelve_422()
    {
        var body = new
        {
            anio = 2030,
            mes = 2,
            fechaInicio = new DateTime(2030, 2, 1),
            fechaFin = new DateTime(2030, 2, 28),
            fechaInicioSolicitud = new DateTime(2030, 2, 1),
            fechaFinSolicitud = new DateTime(2030, 2, 5),
        };
        await _cliente.PostAsJsonAsync("/api/v1/periodos", body);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/periodos", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record PeriodoDto(int Id, int Anio, int Mes, string Estado);
}
