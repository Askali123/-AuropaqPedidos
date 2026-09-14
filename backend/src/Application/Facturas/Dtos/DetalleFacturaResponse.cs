namespace AuropaqPedidos.Application.Facturas.Dtos;

public sealed record DetalleFacturaResponse(
    int Id,
    int DetallePedidoProveedorId,
    int ProductoId,
    int CantidadFacturada,
    decimal PrecioUnitario,
    decimal Subtotal);
