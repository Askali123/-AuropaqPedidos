using System.Net;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Guardia de regresión (docs/2026-09-17-tareas.md, sección 4 idea #3): PedidosProveedorController,
// EntregasController y FacturasController no tenían ningún `[Authorize]` hasta el 2026-09-17
// (a diferencia de RequisicionesController, ver AutorizacionFlujoTests) — cualquier request sin
// JWT podía crear pedidos, entregas y facturas. Agregadas primero como `Skip` (documentando el
// hallazgo sin romper el build, motivo P1-1/P1-2/P1-3 pendientes de P1-4: catalogar los permisos
// que faltaban para Enviar/Cerrar/Cancelar/Anular, `06-seguridad.md §53`).
//
// Activadas el mismo día (RN-063/ADR-066, P1-4 decidido y P1 implementado): ahora confirman que
// los tres controllers exigen JWT real.
public sealed class AutorizacionPedidosEntregasFacturasFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public AutorizacionPedidosEntregasFacturasFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<EscenarioPedidoProveedor> NuevoEscenarioPedidoAsync(int numero, int cantidadNecesaria = 85)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await EscenarioPedidoProveedor.CrearAsync(db, numero, cantidadNecesaria);
    }

    private async Task<EscenarioFactura> NuevoEscenarioFacturaAsync(int numero, int cantidadPedida)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await EscenarioFactura.CrearAsync(db, numero, cantidadPedida);
    }

    [Fact]
    public async Task Crear_pedido_proveedor_sin_jwt_debe_devolver_401()
    {
        var escenario = await NuevoEscenarioPedidoAsync(9001);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-9001"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Enviar_pedido_proveedor_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.PostAsync("/api/v1/pedidos-proveedor/1/enviar", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_entrega_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor/1/entregas", new
        {
            numeroRemision = "REM-9001",
            observacion = (string?)null
        });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Anular_entrega_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.PostAsync("/api/v1/entregas/1/anular", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_factura_sin_jwt_debe_devolver_401()
    {
        var escenario = await NuevoEscenarioFacturaAsync(9002, cantidadPedida: 50);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.ProveedorId,
            numeroFactura = "FAC-9001"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Anular_factura_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.PostAsync("/api/v1/facturas/1/anular", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    // I4-1 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): guardia de regresión para los
    // GET agregados en I1-2/I1-3/I1-4 — mismo criterio que los "sin_jwt" de arriba: no hace falta
    // que el recurso exista, [Authorize] rechaza antes de llegar al caso de uso.

    [Fact]
    public async Task Listar_pedidos_proveedor_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/pedidos-proveedor");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_pedido_proveedor_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/pedidos-proveedor/1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_entregas_de_pedido_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/pedidos-proveedor/1/entregas");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_entrega_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/entregas/1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_facturas_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/facturas?pedidoProveedorId=1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_factura_sin_jwt_debe_devolver_401()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/facturas/1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_y_obtener_pedido_proveedor_con_jwt_valido_sin_PEDIDO_VER_devuelven_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 9010);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 9010, escenario.EmpresaId); // sin PEDIDO_VER

        var listar = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/pedidos-proveedor", token));
        var obtener = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/pedidos-proveedor/1", token));

        Assert.Equal(HttpStatusCode.Forbidden, listar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, obtener.StatusCode);
    }

    [Fact]
    public async Task Listar_entregas_y_obtener_entrega_con_jwt_valido_sin_ENTREGA_VER_devuelven_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 9011);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 9011, escenario.EmpresaId); // sin ENTREGA_VER

        var listarEntregas = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/pedidos-proveedor/1/entregas", token));
        var obtenerEntrega = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/entregas/1", token));

        Assert.Equal(HttpStatusCode.Forbidden, listarEntregas.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, obtenerEntrega.StatusCode);
    }

    [Fact]
    public async Task Listar_y_obtener_factura_con_jwt_valido_sin_FACTURA_VER_devuelven_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, 9012);
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 9012, escenario.EmpresaId); // sin FACTURA_VER

        var listar = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/facturas?pedidoProveedorId=1", token));
        var obtener = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/facturas/1", token));

        Assert.Equal(HttpStatusCode.Forbidden, listar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, obtener.StatusCode);
    }

    private static HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }
}
