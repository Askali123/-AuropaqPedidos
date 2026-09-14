namespace AuropaqPedidos.Application.Consolidaciones.Dtos;

// RN-029, 04-base-datos.md §25: trazabilidad hacia el detalle de requisición que originó la
// cantidad asignada (mismos campos documentados para AsignacionConsolidacion).
public sealed record AsignacionConsolidacionResponse(int Id, int DetalleRequisicionId, int Cantidad);
