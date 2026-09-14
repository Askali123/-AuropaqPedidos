namespace AuropaqPedidos.Application.Requisiciones.Dtos;

// Nombre y forma tomados de docs/05-api.md §19/§38.
public sealed record AgregarDetalleRequisicionRequest(int ProductoId, int CantidadSolicitada, string? Observacion);
