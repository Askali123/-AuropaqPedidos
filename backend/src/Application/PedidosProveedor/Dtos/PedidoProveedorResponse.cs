namespace AuropaqPedidos.Application.PedidosProveedor.Dtos;

public sealed record PedidoProveedorResponse(
    int Id,
    int ConsolidacionId,
    int ProveedorId,
    string NumeroPedido,
    DateTime FechaPedido,
    DateTime? FechaEntregaEstimada,
    string Estado,
    string? Observacion,
    IReadOnlyList<DetallePedidoProveedorResponse> Detalles);
