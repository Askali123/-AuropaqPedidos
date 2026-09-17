import { apiClient } from "./apiClient";
import type { AgregarDetalleEntregaRequest, AgregarDistribucionEntregaRequest, Entrega } from "../types/entrega";

// Encapsula el consumo de EntregasController (backend, docs/05-api.md §33). Exige JWT +
// ENTREGA_VER (obtener), ENTREGA_REGISTRAR (agregar detalle/distribución) o ENTREGA_ANULAR —
// RN-063/ADR-066, apiClient.ts adjunta el token automáticamente. La creación de la cabecera vive
// en pedidosProveedorService.crearEntrega (ruta anidada bajo el pedido).
export const entregasService = {
  obtener: (entregaId: number): Promise<Entrega> => apiClient.get<Entrega>(`/api/v1/entregas/${entregaId}`),

  agregarDetalle: (entregaId: number, request: AgregarDetalleEntregaRequest): Promise<Entrega> =>
    apiClient.post<Entrega>(`/api/v1/entregas/${entregaId}/detalles`, request),

  agregarDistribucion: (
    entregaId: number,
    detalleId: number,
    request: AgregarDistribucionEntregaRequest,
  ): Promise<Entrega> => apiClient.post<Entrega>(`/api/v1/entregas/${entregaId}/detalles/${detalleId}/distribuciones`, request),

  anular: (entregaId: number): Promise<Entrega> => apiClient.post<Entrega>(`/api/v1/entregas/${entregaId}/anular`),
};
