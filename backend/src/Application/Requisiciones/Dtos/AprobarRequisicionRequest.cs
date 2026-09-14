namespace AuropaqPedidos.Application.Requisiciones.Dtos;

// Nombre y forma tomados de docs/05-api.md §24.2/§38.
public sealed record AprobarRequisicionRequest(string? Observacion);
