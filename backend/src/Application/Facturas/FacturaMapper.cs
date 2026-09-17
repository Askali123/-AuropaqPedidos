using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Facturas;

// Traduce las entidades de Domain a los DTOs de salida. Ningún caso de uso devuelve
// directamente una entidad de Domain (mismo criterio que los demás Mapper existentes).
internal static class FacturaMapper
{
    public static FacturaResponse AResponse(Factura factura) => new(
        Id: factura.Id,
        ProveedorId: factura.Proveedor.Id,
        PedidoProveedorId: factura.PedidoProveedor.Id,
        NumeroFactura: factura.NumeroFactura,
        UsuarioCreacionId: factura.UsuarioCreacionId,
        FechaFactura: factura.FechaFactura,
        Subtotal: factura.Subtotal,
        Impuestos: factura.Impuestos,
        Total: factura.Total,
        Estado: factura.Estado.ToString(),
        Observacion: factura.Observacion,
        Detalles: factura.Detalles.Select(ADetalleResponse).ToList());

    private static DetalleFacturaResponse ADetalleResponse(DetalleFactura detalle) => new(
        Id: detalle.Id,
        DetallePedidoProveedorId: detalle.DetallePedidoOrigen.Id,
        ProductoId: detalle.Producto.Id,
        CantidadFacturada: detalle.CantidadFacturada,
        PrecioUnitario: detalle.PrecioUnitario,
        Subtotal: detalle.Subtotal);
}
