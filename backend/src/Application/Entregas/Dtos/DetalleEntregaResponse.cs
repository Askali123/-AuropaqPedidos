namespace AuropaqPedidos.Application.Entregas.Dtos;

public sealed record DetalleEntregaResponse(
    int Id,
    int DetallePedidoProveedorId,
    int ProductoId,
    int CantidadEntregada,
    IReadOnlyList<DistribucionEntregaResponse> Distribuciones);
