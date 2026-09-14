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

// Encapsula el consumo de RequisicionesController (backend, docs/05-api.md §54.4, filas 1-12:
// crear/recuperar, agregar/actualizar/eliminar detalle, agregar/modificar/eliminar distribución,
// guardar borrador, enviar, iniciar revisión, aprobar, devolver. Esto cubre todas las
// transiciones de estado documentadas para Requisicion.
export const requisicionesService = {
  // Único endpoint de este módulo que exige headers de identidad (placeholder temporal,
  // docs/05-api.md §54.1): X-Usuario-Id y X-Empresa-Id, mientras no exista autenticación real.
  iniciarOContinuar: (empresaId: number, usuarioId: number, request: CrearRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>("/api/v1/requisiciones", request, {
      "X-Usuario-Id": String(usuarioId),
      "X-Empresa-Id": String(empresaId),
    }),

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

  // docs/05-api.md §54.4, fila 9. Exige X-Usuario-Id (mismo placeholder temporal de §54.1);
  // sin body. Válido tanto desde Borrador (envío inicial) como desde Devuelta (reenvío) — el
  // backend decide, el frontend no distingue entre ambos casos.
  enviar: (requisicionId: number, usuarioId: number): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/enviar`, undefined, {
      "X-Usuario-Id": String(usuarioId),
    }),

  // docs/05-api.md §54.4, fila 10 / §54.7. Exige X-Usuario-Id, sin body. §54.7 documenta que
  // ningún RN-XXX/TASK-XXX define todavía quién (qué rol) debe ver este botón — solo que el
  // endpoint ya funciona y el Frontend puede construirlo. No se inventa aquí ningún criterio de
  // visibilidad por rol/permiso: se usa el mismo criterio que el resto de la pantalla (estado de
  // la Requisicion), porque no existe ningún otro mecanismo de permisos implementado todavía.
  iniciarRevision: (requisicionId: number, usuarioId: number): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/iniciar-revision`, undefined, {
      "X-Usuario-Id": String(usuarioId),
    }),

  // docs/05-api.md §54.4, fila 11. Exige X-Usuario-Id; body con "observacion" opcional
  // (AprobarRequisicionRequest, Application). RN-017 dice "el usuario autorizado puede
  // aprobar" pero no define ningún rol concreto (mismo vacío ya documentado en §54.7 para
  // iniciar-revision) — no se inventa ningún control de permisos, se usa el mismo criterio de
  // estado que el resto de la pantalla.
  aprobar: (requisicionId: number, usuarioId: number, request: AprobarRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/aprobar`, request, {
      "X-Usuario-Id": String(usuarioId),
    }),

  // docs/05-api.md §54.4, fila 12. Exige X-Usuario-Id; body con "motivo" obligatorio, no vacío
  // (DevolverRequisicionRequest, Application) — Requisicion.Devolver() (Domain) rechaza con 422
  // si viene vacío/blanco. RN-018 dice "el revisor puede devolver" sin definir ningún rol
  // concreto (mismo vacío ya documentado para iniciar-revision/aprobar) — no se inventa ningún
  // control de permisos nuevo.
  devolver: (requisicionId: number, usuarioId: number, request: DevolverRequisicionRequest): Promise<Requisicion> =>
    apiClient.post<Requisicion>(`/api/v1/requisiciones/${requisicionId}/devolver`, request, {
      "X-Usuario-Id": String(usuarioId),
    }),
};
