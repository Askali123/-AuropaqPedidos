namespace AuropaqPedidos.Application.Entregas.Dtos;

public sealed record AgregarDetalleEntregaRequest(int DetallePedidoProveedorId, int CantidadEntregada);
