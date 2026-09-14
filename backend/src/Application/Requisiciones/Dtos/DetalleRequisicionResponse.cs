namespace AuropaqPedidos.Application.Requisiciones.Dtos;

public sealed record DetalleRequisicionResponse(
    int Id,
    int ProductoId,
    int CantidadSolicitada,
    string? Observacion,
    int CantidadDistribuida,
    bool DistribucionCompleta,
    IReadOnlyList<DistribucionRequisicionResponse> Distribuciones);
