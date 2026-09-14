namespace AuropaqPedidos.Application.Entregas.Dtos;

public sealed record DistribucionEntregaResponse(
    int Id,
    int SedeId,
    int Cantidad,
    string? DireccionEntrega,
    string? CiudadEntrega,
    string? ContactoEntrega);
