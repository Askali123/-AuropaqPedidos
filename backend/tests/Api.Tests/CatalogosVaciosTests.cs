using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Clase dedicada (con su propia ApiWebApplicationFactory / base de datos recién migrada, sin
// ningún dato sembrado por otras pruebas) para demostrar que los endpoints de solo lectura
// devuelven listas vacías reales cuando no hay datos, en vez de error. CatalogosFlujoTests
// comparte la base de datos con otras clases sembradas, así que no sirve para probar este caso.
//
// RN-059/060 (punto 8, 2026-09-15): estos 3 endpoints ya exigen JWT + permiso. Emitir un JWT
// requiere un Usuario, que a su vez requiere una Empresa (FK obligatoria) — por eso ya no existe
// un estado con "cero Empresas": el mínimo real es "las Empresas bootstrap creadas para poder
// autenticar a los usuarios de esta clase de prueba (una por [Fact], misma Empresa compartida
// vía IClassFixture)". GET /empresas no tiene alcance por empresa (RN-059: Organización es
// administrativa/global), así que la prueba de Empresas verifica "solo existen las Empresas
// bootstrap, ninguna de negocio real" en vez de "exactamente una". Productos/Periodos sí pueden
// seguir vacíos de verdad: no dependen de que exista ninguno para que el Usuario de la prueba
// pueda autenticarse.
public sealed class CatalogosVaciosTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public CatalogosVaciosTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<(int empresaId, string token)> EmpresaYTokenDeAutenticacionAsync(int numero, params string[] permisos)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresa = new AuropaqPedidos.Domain.Entities.Empresa(500_000 + numero, $"Empresa bootstrap {numero}");
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, numero, empresa.Id, permisos);
        return (empresa.Id, token);
    }

    [Fact]
    public async Task Listar_empresas_sin_datos_de_negocio_devuelve_solo_empresas_bootstrap()
    {
        var (empresaId, token) = await EmpresaYTokenDeAutenticacionAsync(1, "ORGANIZACION_VER");
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/empresas");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<EmpresaDto>>>();

        Assert.NotNull(cuerpo);
        // GET /empresas es global (sin alcance, RN-059) — dentro de esta clase, las otras
        // pruebas ([Fact] hermanas, misma Empresa de BD vía IClassFixture) también crean su
        // propia Empresa bootstrap para poder autenticarse; solo importa que la propia esté
        // presente y que ninguna sea un dato de negocio real (todas por debajo de 500_000+N).
        Assert.Contains(cuerpo!.Data, e => e.Id == empresaId);
        Assert.All(cuerpo.Data, e => Assert.True(e.Id >= 500_001, $"Empresa {e.Id} no es una Empresa bootstrap de autenticación."));
    }

    [Fact]
    public async Task Listar_productos_sin_datos_devuelve_lista_vacia()
    {
        var (_, token) = await EmpresaYTokenDeAutenticacionAsync(2, "PRODUCTO_VER");
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/productos");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<object>>>();

        Assert.NotNull(cuerpo);
        Assert.Empty(cuerpo!.Data);
    }

    [Fact]
    public async Task Listar_periodos_sin_datos_devuelve_lista_vacia()
    {
        var (_, token) = await EmpresaYTokenDeAutenticacionAsync(3, "PERIODO_VER");
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/periodos");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await _cliente.SendAsync(solicitud);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<List<object>>>();

        Assert.NotNull(cuerpo);
        Assert.Empty(cuerpo!.Data);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record EmpresaDto(int Id, string Nombre, bool Activo);
}
