// Corresponden exactamente al contrato real documentado en docs/05-api.md §54.3/§54.5
// (backend: Application.Requisiciones.Dtos.RequisicionResponse y tipos relacionados). El valor
// de "estado" es el nombre del enum de C# tal cual (PascalCase), no el texto en mayúsculas de
// la documentación conceptual original (§17-27).
export type RequisicionEstado = "Borrador" | "Enviada" | "EnRevision" | "Devuelta" | "Aprobada";

// EsEditable (Domain) es verdadero únicamente en estos dos estados — el Frontend no debe
// inventar su propio criterio de "se puede editar".
export const ESTADOS_EDITABLES: readonly RequisicionEstado[] = ["Borrador", "Devuelta"];

export interface DistribucionRequisicion {
  id: number;
  sedeId: number;
  cantidad: number;
}

export interface DetalleRequisicion {
  id: number;
  productoId: number;
  cantidadSolicitada: number;
  observacion: string | null;
  cantidadDistribuida: number;
  distribucionCompleta: boolean;
  distribuciones: DistribucionRequisicion[];
}

export interface HistorialRequisicion {
  estadoAnterior: string;
  estadoNuevo: string;
  usuarioId: number;
  fecha: string; // ISO 8601
  comentario: string | null;
}

export interface Requisicion {
  id: number;
  empresaId: number;
  periodoId: number;
  usuarioCreacionId: number;
  estado: RequisicionEstado;
  fechaCreacion: string; // ISO 8601
  fechaEnvio: string | null;
  detalles: DetalleRequisicion[];
  historial: HistorialRequisicion[];
}

// docs/05-api.md §54.4, fila 1. EmpresaId no va en el body: viaja en el header X-Empresa-Id.
export interface CrearRequisicionRequest {
  periodoId: number;
}

// docs/05-api.md §54.4, fila 2.
export interface AgregarDetalleRequisicionRequest {
  productoId: number;
  cantidadSolicitada: number;
  observacion?: string | null;
}

// docs/05-api.md §54.4, fila 3. Cada campo ausente/null significa "no modificarlo" — nunca
// "borrarlo" (ActualizarDetalleRequisicionRequest, backend).
export interface ActualizarDetalleRequisicionRequest {
  cantidadSolicitada?: number | null;
  observacion?: string | null;
}

// docs/05-api.md §54.4, fila 5 (AgregarDistribucionRequest, Api/Controllers/Dtos).
export interface AgregarDistribucionRequisicionRequest {
  sedeId: number;
  cantidad: number;
}

// docs/05-api.md §54.4, fila 6 (ModificarDistribucionRequest, Api/Controllers/Dtos).
export interface ModificarDistribucionRequisicionRequest {
  nuevaCantidad: number;
}

// docs/05-api.md §54.4, fila 11 (AprobarRequisicionRequest, Application/Requisiciones/Dtos).
export interface AprobarRequisicionRequest {
  observacion?: string | null;
}

// docs/05-api.md §54.4, fila 12 (DevolverRequisicionRequest, Application/Requisiciones/Dtos).
// A diferencia de AprobarRequisicionRequest, "motivo" es obligatorio (string, no string | null) —
// Requisicion.Devolver() (Domain) rechaza vacío/blanco con 422.
export interface DevolverRequisicionRequest {
  motivo: string;
}
