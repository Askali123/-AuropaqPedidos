namespace AuropaqPedidos.Application.Consolidaciones.Dtos;

public sealed record DetalleConsolidacionResponse(
    int Id,
    int ProductoId,
    int CantidadNecesaria,
    IReadOnlyList<AsignacionConsolidacionResponse> Asignaciones);
