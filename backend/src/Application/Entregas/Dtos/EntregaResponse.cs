namespace AuropaqPedidos.Application.Entregas.Dtos;

public sealed record EntregaResponse(
    int Id,
    int PedidoProveedorId,
    DateTime FechaEntrega,
    string NumeroRemision,
    string Estado,
    string? Observacion,
    IReadOnlyList<DetalleEntregaResponse> Detalles);
