namespace AuropaqPedidos.Application.Consolidaciones.Dtos;

// Estado no tiene valores documentados para Consolidacion (04-base-datos.md §23, a diferencia
// de RequisicionEstado); igual que Periodo.Estado, queda como texto libre que decide el
// llamador, sin que Application invente un valor por defecto.
public sealed record CrearConsolidacionRequest(int PeriodoId, string Estado, string? Observacion = null);
