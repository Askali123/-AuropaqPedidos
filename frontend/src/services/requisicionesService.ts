import { apiClient } from "./apiClient";
import type {
  ActualizarDetalleRequisicionRequest,
  AgregarDetalleRequisicionRequest,
  AgregarDistribucionRequisicionRequest,
  AprobarRequisicionRequest,
  CrearRequisicionRequest,
  DevolverRequisicionRequest,
  ModificarDistribucionRequisicionRequest,
  Requisicion,
} from "../types/requisicion";

// Encapsula el consumo de RequisicionesController (backend, docs/05-api.md §54.4 + punto 8,
// 2026-09-15: autorización real en todas las rutas). El actor y la empresa se derivan siempre
// del JWT en el backend — ningún método de aquí recibe ni envía usuarioId/empresaId; apiClient.ts
// adjunta el token automáticamente.
export const requisicionesService = {
  listar: (): Promise<Requisicion[]> => apiClient.get<Requisicion[]>("/api/v1/requisiciones"),

  listarPendientesDeRevision: (): Promise<Requisicion[]> =>
    apiClient.get<Requisicion[]>("/api/v1/requisiciones/pendientes-revision"),

  obtener: (requisicionId: number): Promise<Requisicion> =>
    apiClient.get<Requisicion>(`/api/v1/requisiciones/${requisicionId}`),

  iniciarOContinuar: (request: CrearRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>("/api/v1/requisiciones", request),

  agregarDetalle: (requisicionId: number, request: AgregarDetalleRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/detalles`, request),

  actualizarDetalle: (
    requisicionId: number,
    detalleId: number,
    request: ActualizarDetalleRequisicionRequest,
  ): Promise<Requisicion> =>
    apiClient.put<Requisicion>(`/api/v1/requisiciones/${requisicionId}/detalles/${detalleId}`, request),

  eliminarDetalle: (requisicionId: number, detalleId: number): Promise<Requisicion> =>
    apiClient.delete<Requisicion>(`/api/v1/requisiciones/${requisicionId}/detalles/${detalleId}`),

  agregarDistribucion: (
    requisicionId: number,
    detalleId: number,
    request: AgregarDistribucionRequisicionRequest,
  ): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/detalles/${detalleId}/distribuciones`, request),

  modificarDistribucion: (
    requisicionId: number,
    detalleId: number,
    distribucionId: number,
    request: ModificarDistribucionRequisicionRequest,
  ): Promise<Requisicion> =>
    apiClient.put<Requisicion>(
      `/api/v1/requisiciones/${requisicionId}/detalles/${detalleId}/distribuciones/${distribucionId}`,
      request,
    ),

  eliminarDistribucion: (requisicionId: number, detalleId: number, distribucionId: number): Promise<Requisicion> =>
    apiClient.delete<Requisicion>(
      `/api/v1/requisiciones/${requisicionId}/detalles/${detalleId}/distribuciones/${distribucionId}`,
    ),

  guardarBorrador: (requisicionId: number): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/guardar`),

  enviar: (requisicionId: number): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/enviar`),

  iniciarRevision: (requisicionId: number): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/iniciar-revision`),

  aprobar: (requisicionId: number, request: AprobarRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/aprobar`, request),

  devolver: (requisicionId: number, request: DevolverRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/devolver`, request),
};
