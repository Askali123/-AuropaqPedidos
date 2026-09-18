using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de PedidoProveedor (cierre documental 2026-09-11, D-01 a D-13) a
// través de la Api real (Controllers + Application + Infrastructure + SQL Server), según
// 03-arquitectura.md §42. Sin EntregasController (fuera de alcance de esta tarea): para probar
// el cierre (que exige estado ENTREGADO) se siembra la entrega directamente en la base de
// datos, igual que EscenarioFactura siembra el grafo previo a Factura.
//
// Autorización real agregada 2026-09-17 (RN-063/ADR-066): estos endpoints ya exigen JWT +
// PEDIDO_CREAR/ENVIAR/CERRAR/CANCELAR (sin alcance por empresa, CLAUDE.md §27). Este archivo
// prueba comportamiento de negocio, no autorización granular (eso lo cubre
// AutorizacionPedidosEntregasFacturasFlujoTests) — un único token con los cuatro permisos,
// autenticado en `_cliente` por defecto desde `NuevoEscenarioAsync`, alcanza para todo el flujo.
public sealed class PedidosProveedorFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public PedidosProveedorFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<EscenarioPedidoProveedor> NuevoEscenarioAsync(int numero, int cantidadNecesaria = 85)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await EscenarioPedidoProveedor.CrearAsync(db, numero, cantidadNecesaria);

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, escenario.EmpresaId,
            "PEDIDO_CREAR", "PEDIDO_ENVIAR", "PEDIDO_CERRAR", "PEDIDO_CANCELAR", "PEDIDO_VER", "ENTREGA_VER");
        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return escenario;
    }

    [Fact]
    public async Task Crear_pedido_agregar_detalle_y_distribucion_y_enviarlo()
    {
        var escenario = await NuevoEscenarioAsync(1);

        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-001"
        });
        Assert.Equal(HttpStatusCode.Created, respuestaCrear.StatusCode);
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Equal("Borrador", pedido!.Data.Estado);

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles", new
        {
            detalleConsolidacionId = escenario.DetalleConsolidacionId,
            cantidadPedida = 100,
            precioUnitario = 12.5m
        });
        Assert.Equal(HttpStatusCode.Created, respuestaDetalle.StatusCode);
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        var detalle = Assert.Single(conDetalle!.Data.Detalles);
        Assert.Equal(85, detalle.CantidadNecesaria);
        Assert.Equal(100, detalle.CantidadPedida);

        var respuestaDistribucion = await _cliente.PostAsJsonAsync(
            $"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles/{detalle.Id}/distribuciones", new
            {
                sedeId = escenario.SedeId,
                cantidad = 100
            });
        Assert.Equal(HttpStatusCode.Created, respuestaDistribucion.StatusCode);
        var conDistribucion = await respuestaDistribucion.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Single(conDistribucion!.Data.Detalles[0].Distribuciones);

        var respuestaEnviar = await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/enviar", content: null);
        Assert.Equal(HttpStatusCode.OK, respuestaEnviar.StatusCode);
        var enviado = await respuestaEnviar.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Equal("Enviado", enviado!.Data.Estado);
    }

    // TASK-105 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo D1/F4).
    [Fact]
    public async Task Agregar_detalle_incluye_el_codigo_del_proveedor_cuando_existe_la_relacion()
    {
        var escenario = await NuevoEscenarioAsync(70);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var producto = await db.Productos.SingleAsync(p => p.Id == escenario.EmpresaId);
            var proveedor = await db.Proveedores.SingleAsync(p => p.Id == escenario.ProveedorId);
            db.ProductosProveedores.Add(new ProductoProveedor(70, producto, proveedor, "COD-PROV-070"));
            await db.SaveChangesAsync();
        }

        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-070"
        });
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/detalles", new
        {
            detalleConsolidacionId = escenario.DetalleConsolidacionId,
            cantidadPedida = 100,
        });

        Assert.Equal(HttpStatusCode.Created, respuestaDetalle.StatusCode);
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Equal("COD-PROV-070", conDetalle!.Data.Detalles[0].CodigoProveedorUtilizado);
    }

    [Fact]
    public async Task Crear_pedido_con_consolidacion_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(2);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = 999999,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-002"
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_permite_dos_pedidos_con_el_mismo_numero_para_el_mismo_proveedor()
    {
        var escenario = await NuevoEscenarioAsync(3);
        await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-DUP"
        });

        // Segunda consolidación del mismo Escenario "reutilizada" no aplica aquí (cada
        // consolidación pertenece a un único pedido); se reintenta con la misma consolidación
        // para aislar exclusivamente la colisión de NumeroPedido+Proveedor (D-09).
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-DUP"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task No_permite_cerrar_un_pedido_que_no_esta_entregado()
    {
        var escenario = await NuevoEscenarioAsync(4);
        var pedidoId = await CrearYEnviarPedidoAsync(escenario, "PO-004");

        var respuesta = await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cerrar", content: null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Cancelar_un_pedido_enviado_es_valido()
    {
        var escenario = await NuevoEscenarioAsync(5);
        var pedidoId = await CrearYEnviarPedidoAsync(escenario, "PO-005");

        var respuesta = await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cancelar", content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cancelado = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Equal("Cancelado", cancelado!.Data.Estado);
    }

    [Fact]
    public async Task Cerrar_pedido_inexistente_devuelve_404()
    {
        await NuevoEscenarioAsync(7); // solo para autenticar _cliente (mismo helper que el resto).

        var respuesta = await _cliente.PostAsync("/api/v1/pedidos-proveedor/999999/cerrar", content: null);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    // D-08/RN-044: cierre explícito, sin requerir factura. Como no hay EntregasController, la
    // entrega que deja el pedido en ENTREGADO se siembra directamente en la base de datos.
    [Fact]
    public async Task Cierra_un_pedido_entregado_sin_ninguna_factura_asociada()
    {
        var escenario = await NuevoEscenarioAsync(6);
        var pedidoId = await CrearYEnviarPedidoAsync(escenario, "PO-006", cantidadPedida: 85);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var pedido = db.PedidosProveedor
                .Include(p => p.Detalles)
                .Single(p => p.Id == pedidoId);
            var entrega = new AuropaqPedidos.Domain.Entities.Entrega(90000 + pedidoId, pedido, usuarioCreacionId: 1, DateTime.UtcNow, "REM-SEED");
            entrega.AgregarDetalle(90000 + pedidoId, pedido.Detalles[0], cantidadEntregada: 85, cantidadYaEntregadaEnOtrasEntregas: 0);
            pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: false);
            db.Entregas.Add(entrega);
            await db.SaveChangesAsync();
        }

        var respuesta = await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cerrar", content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cerrado = await respuesta.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        Assert.Equal("Cerrado", cerrado!.Data.Estado);
    }

    [Fact]
    public async Task Listar_pedidos_filtrados_por_consolidacion_devuelve_solo_los_de_esa_consolidacion()
    {
        var escenario = await NuevoEscenarioAsync(8);
        var pedidoId = await CrearYEnviarPedidoAsync(escenario, "PO-008");

        var respuesta = await _cliente.GetAsync($"/api/v1/pedidos-proveedor?consolidacionId={escenario.ConsolidacionId}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var lista = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<PedidoProveedorDto>>>();
        Assert.Contains(lista!.Data, p => p.Id == pedidoId);
    }

    [Fact]
    public async Task Obtener_pedido_inexistente_devuelve_404()
    {
        await NuevoEscenarioAsync(9); // solo para autenticar _cliente.

        var respuesta = await _cliente.GetAsync("/api/v1/pedidos-proveedor/999999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_entregas_de_un_pedido_sin_entregas_devuelve_lista_vacia()
    {
        var escenario = await NuevoEscenarioAsync(10);
        var pedidoId = await CrearYEnviarPedidoAsync(escenario, "PO-010");

        var respuesta = await _cliente.GetAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var lista = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<object>>>();
        Assert.Empty(lista!.Data);
    }

    private async Task<int> CrearYEnviarPedidoAsync(EscenarioPedidoProveedor escenario, string numeroPedido, int cantidadPedida = 100)
    {
        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido
        });
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/detalles", new
        {
            detalleConsolidacionId = escenario.DetalleConsolidacionId,
            cantidadPedida
        });
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        await _cliente.PostAsJsonAsync(
            $"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles/{conDetalle!.Data.Detalles[0].Id}/distribuciones", new
            {
                sedeId = escenario.SedeId,
                cantidad = cantidadPedida
            });

        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/enviar", content: null);

        return pedido.Data.Id;
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record PedidoProveedorDto(
        int Id, int ConsolidacionId, int ProveedorId, string NumeroPedido, string Estado, IReadOnlyList<DetallePedidoProveedorDto> Detalles);

    private sealed record DetallePedidoProveedorDto(
        int Id, int CantidadNecesaria, int CantidadPedida, string? CodigoProveedorUtilizado, IReadOnlyList<object> Distribuciones);
}
