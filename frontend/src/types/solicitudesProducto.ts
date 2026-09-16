// Corresponden exactamente a los DTOs reales documentados en docs/05-api.md §28
// (backend: Application.Catalogo.Dtos.*). "Estado" es el nombre del enum de C# tal cual
// (PascalCase: "Pendiente"/"Homologado"/"Creado"/"Rechazado"), mismo criterio que
// RequisicionResponse.Estado — no se traduce a mayúsculas.
export type SolicitudProductoEstado = "Pendiente" | "Homologado" | "Creado" | "Rechazado";

export interface SolicitudProductoCatalogo {
  id: number;
  empresaId: number;
  usuarioId: number;
  nombreSolicitado: string;
  descripcion: string | null;
  observacion: string | null;
  estado: SolicitudProductoEstado;
  productoResultanteId: number | null;
  fechaSolicitud: string; // ISO 8601
  fechaResolucion: string | null;
  usuarioResolucionId: number | null;
  motivoResolucion: string | null;
}

// docs/05-api.md §28.1.
export interface SolicitarProductoRequest {
  nombreSolicitado: string;
  descripcion?: string | null;
  observacion?: string | null;
}

// docs/05-api.md §28.3.
export interface HomologarProductoRequest {
  productoId: number;
}

// docs/05-api.md §28.4. CategoriaId/UnidadMedidaId obligatorios (RN-021, invariante de Domain);
// Nombre/Descripcion del producto nuevo se toman de la propia solicitud, no se piden aquí.
export interface CrearProductoDesdeSolicitudRequest {
  categoriaId: number;
  unidadMedidaId: number;
  codigoInterno?: string | null;
}

// docs/05-api.md §28.5.
export interface RechazarSolicitudRequest {
  motivo: string;
}
