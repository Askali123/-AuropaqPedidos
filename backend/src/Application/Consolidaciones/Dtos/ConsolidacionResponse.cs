namespace AuropaqPedidos.Application.Consolidaciones.Dtos;

public sealed record ConsolidacionResponse(
    int Id,
    int PeriodoId,
    int UsuarioCreacionId,
    string Estado,
    DateTime FechaCreacion,
    string? Observacion,
    IReadOnlyList<DetalleConsolidacionResponse> Detalles);
