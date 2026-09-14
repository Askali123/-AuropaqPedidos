namespace AuropaqPedidos.Application.Facturas.Dtos;

public sealed record AgregarDetalleFacturaRequest(int DetallePedidoProveedorId, int CantidadFacturada, decimal PrecioUnitario);
