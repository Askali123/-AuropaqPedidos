// Corresponden exactamente a AuropaqPedidos.Application.Consolidaciones.Dtos (backend,
// docs/05-api.md §31). Estado es texto libre (04-base-datos.md §23, sin enum documentado) —
// el frontend no restringe los valores posibles, solo envía lo que el usuario escribe.
export interface AsignacionConsolidacion {
  id: number;
  detalleRequisicionId: number;
  cantidad: number;
}

export interface DetalleConsolidacion {
  id: number;
  productoId: number;
  cantidadNecesaria: number;
  asignaciones: AsignacionConsolidacion[];
}

export interface Consolidacion {
  id: number;
  periodoId: number;
  usuarioCreacionId: number;
  estado: string;
  fechaCreacion: string; // ISO 8601
  observacion: string | null;
  detalles: DetalleConsolidacion[];
}

// docs/05-api.md §31 (CrearConsolidacionRequest, Application/Consolidaciones/Dtos). Estado no
// tiene un valor por defecto documentado — el usuario lo escribe (mismo criterio que
// Periodo.Estado); "GENERADA" es solo lo que se usó hasta ahora manualmente, no un valor fijo
// del backend.
export interface CrearConsolidacionRequest {
  periodoId: number;
  estado: string;
  observacion?: string | null;
}
