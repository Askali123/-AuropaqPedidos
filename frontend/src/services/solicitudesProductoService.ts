import { apiClient } from "./apiClient";
import type {
  CrearProductoDesdeSolicitudRequest,
  HomologarProductoRequest,
  RechazarSolicitudRequest,
  SolicitarProductoRequest,
  SolicitudProductoCatalogo,
} from "../types/solicitudesProducto";

// Encapsula el consumo de SolicitudesProductoController (backend, docs/05-api.md §28). Solicitar
// exige PRODUCTO_SOLICITAR (rol Solicitante); listar/homologar/crear-producto/rechazar exigen
// PRODUCTO_VER/PRODUCTO_CREAR (gestor de catálogo, sin alcance por empresa — 06-seguridad.md
// §12/§52/§53). apiClient.ts adjunta el JWT automáticamente. Sin "quitar"/deshacer una
// resolución: no existe en el backend.
export const solicitudesProductoService = {
  solicitar: (request: SolicitarProductoRequest): Promise<SolicitudProductoCatalogo> =>
    apiClient.post<SolicitudProductoCatalogo>("/api/v1/solicitudes-producto", request),

  listarPendientes: (): Promise<SolicitudProductoCatalogo[]> =>
    apiClient.get<SolicitudProductoCatalogo[]>("/api/v1/solicitudes-producto/pendientes"),

  homologar: (solicitudId: number, request: HomologarProductoRequest): Promise<SolicitudProductoCatalogo> =>
    apiClient.post<SolicitudProductoCatalogo>(`/api/v1/solicitudes-producto/${solicitudId}/homologar`, request),

  crearProducto: (
    solicitudId: number,
    request: CrearProductoDesdeSolicitudRequest,
  ): Promise<SolicitudProductoCatalogo> =>
    apiClient.post<SolicitudProductoCatalogo>(`/api/v1/solicitudes-producto/${solicitudId}/crear-producto`, request),

  rechazar: (solicitudId: number, request: RechazarSolicitudRequest): Promise<SolicitudProductoCatalogo> =>
    apiClient.post<SolicitudProductoCatalogo>(`/api/v1/solicitudes-producto/${solicitudId}/rechazar`, request),
};
