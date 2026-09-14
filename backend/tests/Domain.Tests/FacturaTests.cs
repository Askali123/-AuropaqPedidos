using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class FacturaTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto(int id, string nombre) =>
        new(id, nombre, new Categoria(id, "Aseo"), new UnidadMedida(id, "UNIDAD", "Unidad"));

    // Mismo helper que PedidoProveedorTests/ConsolidacionTests: requisición APROBADA con un
    // único detalle, consolidada, y un PedidoProveedor con un único DetallePedidoProveedor.
    private static (PedidoProveedor Pedido, DetallePedidoProveedor Detalle, Proveedor Proveedor) CrearPedidoConUnDetalle(
        Producto producto, int cantidadPedida)
    {
        var periodo = CrearPeriodo();
        var empresa = new Empresa(1, "Empresa 1");
        var requisicion = new Requisicion(1, empresa, periodo, 10, Fecha);
        var detalleReq = requisicion.AgregarDetalle(1, producto, cantidadPedida);
        var sede = new Sede(1, empresa, "Sede 1");
        requisicion.AgregarDistribucion(1, detalleReq, sede, cantidadPedida);
        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);

        var consolidacion = new Consolidacion(1, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
        consolidacion.AgregarAsignacion(1, 1, requisicion, detalleReq, cantidadPedida);

        var proveedor = new Proveedor(1, "Proveedor Uno");
        var pedido = new PedidoProveedor(1, consolidacion, proveedor, "PO-001", Fecha);
        var detallePedido = pedido.AgregarDetalle(1, consolidacion.Detalles[0], cantidadPedida);

        return (pedido, detallePedido, proveedor);
    }

    private static Factura CrearFactura(PedidoProveedor pedido, Proveedor proveedor) => new(
        1, proveedor, pedido, "F-001", Fecha, impuestos: 19m);

    [Fact]
    public void Crea_una_factura_valida_para_un_pedido_del_mismo_proveedor()
    {
        var (pedido, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);

        var factura = CrearFactura(pedido, proveedor);

        Assert.Equal(proveedor, factura.Proveedor);
        Assert.Equal(pedido, factura.PedidoProveedor);
        Assert.Equal("F-001", factura.NumeroFactura);
        Assert.Equal(19m, factura.Impuestos);
        Assert.Equal(FacturaEstado.Registrada, factura.Estado);
        Assert.Equal(0m, factura.Subtotal);
        Assert.Equal(19m, factura.Total);
        Assert.Empty(factura.Detalles);
    }

    [Fact]
    public void No_permite_crear_una_factura_sin_proveedor()
    {
        var (pedido, _, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new Factura(1, proveedor: null!, pedido, "F-001", Fecha, 19m));
    }

    [Fact]
    public void No_permite_crear_una_factura_sin_pedido()
    {
        var (_, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new Factura(1, proveedor, pedidoProveedor: null!, "F-001", Fecha, 19m));
    }

    [Fact]
    public void No_permite_crear_una_factura_cuyo_proveedor_no_corresponde_al_pedido()
    {
        var (pedido, _, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var otroProveedor = new Proveedor(2, "Otro proveedor");

        Assert.Throws<ReglaDeNegocioException>(() =>
            new Factura(1, otroProveedor, pedido, "F-001", Fecha, 19m));
    }

    [Fact]
    public void No_permite_crear_una_factura_sin_numero_de_factura()
    {
        var (pedido, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new Factura(1, proveedor, pedido, numeroFactura: "", Fecha, 19m));
    }

    [Fact]
    public void Agregar_detalle_calcula_subtotal_de_linea_y_actualiza_totales_de_la_factura()
    {
        var (pedido, detallePedido, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        var detalle = factura.AgregarDetalle(1, detallePedido, cantidadFacturada: 60, precioUnitario: 12.5m, cantidadYaFacturadaEnOtrasFacturas: 0);

        Assert.Equal(60, detalle.CantidadFacturada);
        Assert.Equal(12.5m, detalle.PrecioUnitario);
        Assert.Equal(750m, detalle.Subtotal);
        Assert.Equal(750m, factura.Subtotal);
        Assert.Equal(769m, factura.Total);
    }

    [Fact]
    public void No_permite_agregar_un_detalle_de_pedido_que_no_pertenece_a_esta_factura()
    {
        var (pedidoA, _, proveedorA) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var (_, detalleB, _) = CrearPedidoConUnDetalle(CrearProducto(2, "Jabón"), 20);
        var factura = CrearFactura(pedidoA, proveedorA);

        Assert.Throws<ReglaDeNegocioException>(() =>
            factura.AgregarDetalle(1, detalleB, 10, 5m, 0));
    }

    [Fact]
    public void No_permite_cantidad_facturada_menor_o_igual_a_cero()
    {
        var (pedido, detallePedido, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        Assert.Throws<ReglaDeNegocioException>(() =>
            factura.AgregarDetalle(1, detallePedido, cantidadFacturada: 0, precioUnitario: 5m, cantidadYaFacturadaEnOtrasFacturas: 0));
    }

    [Fact]
    public void No_permite_precio_unitario_menor_o_igual_a_cero()
    {
        var (pedido, detallePedido, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        Assert.Throws<ReglaDeNegocioException>(() =>
            factura.AgregarDetalle(1, detallePedido, cantidadFacturada: 10, precioUnitario: 0m, cantidadYaFacturadaEnOtrasFacturas: 0));
    }

    [Fact]
    public void No_permite_que_la_cantidad_facturada_acumulada_supere_la_cantidad_pedida()
    {
        var (pedido, detallePedido, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        // Ya se facturaron 70 en otra factura del mismo pedido; esta línea intenta agregar 40 más (110 > 100).
        Assert.Throws<ReglaDeNegocioException>(() =>
            factura.AgregarDetalle(1, detallePedido, cantidadFacturada: 40, precioUnitario: 5m, cantidadYaFacturadaEnOtrasFacturas: 70));
    }

    [Fact]
    public void Permite_facturacion_parcial_mientras_no_supere_la_cantidad_pedida()
    {
        var (pedido, detallePedido, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        var detalle = factura.AgregarDetalle(1, detallePedido, cantidadFacturada: 40, precioUnitario: 5m, cantidadYaFacturadaEnOtrasFacturas: 60);

        Assert.Equal(40, detalle.CantidadFacturada);
    }

    // D-05/RN-047 (cierre documental 2026-09-11): estados operativos de Factura.

    [Fact]
    public void Una_factura_se_crea_siempre_registrada()
    {
        var (pedido, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);

        var factura = CrearFactura(pedido, proveedor);

        Assert.Equal(FacturaEstado.Registrada, factura.Estado);
    }

    [Fact]
    public void Anular_pasa_de_registrada_a_anulada()
    {
        var (pedido, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);

        factura.Anular();

        Assert.Equal(FacturaEstado.Anulada, factura.Estado);
    }

    [Fact]
    public void No_permite_anular_una_factura_ya_anulada()
    {
        var (pedido, _, proveedor) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 100);
        var factura = CrearFactura(pedido, proveedor);
        factura.Anular();

        Assert.Throws<ReglaDeNegocioException>(() => factura.Anular());
    }
}
