using System.Net.Http.Json;

namespace Api.Tests;

// Clase dedicada (con su propia ApiWebApplicationFactory / base de datos recién migrada, sin
// ningún dato sembrado por otras pruebas) para demostrar que los endpoints de solo lectura
// devuelven listas vacías reales cuando no hay datos, en vez de error. CatalogosFlujoTests
// comparte la base de datos con otras clases sembradas, así que no sirve para probar este caso.
public sealed class CatalogosVaciosTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _cliente;

    public CatalogosVaciosTests(ApiWebApplicationFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Listar_empresas_sin_datos_devuelve_lista_vacia()
    {
        var respuesta = await _cliente.GetFromJsonAsync<Envoltorio<List<object>>>("/api/v1/empresas");

        Assert.NotNull(respuesta);
        Assert.Empty(respuesta!.Data);
    }

    [Fact]
    public async Task Listar_productos_sin_datos_devuelve_lista_vacia()
    {
        var respuesta = await _cliente.GetFromJsonAsync<Envoltorio<List<object>>>("/api/v1/productos");

        Assert.NotNull(respuesta);
        Assert.Empty(respuesta!.Data);
    }

    [Fact]
    public async Task Listar_periodos_sin_datos_devuelve_lista_vacia()
    {
        var respuesta = await _cliente.GetFromJsonAsync<Envoltorio<List<object>>>("/api/v1/periodos");

        Assert.NotNull(respuesta);
        Assert.Empty(respuesta!.Data);
    }

    private sealed record Envoltorio<T>(T Data);
}
