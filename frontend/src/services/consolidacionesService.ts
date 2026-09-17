import { apiClient } from "./apiClient";
import type { Consolidacion, CrearConsolidacionRequest } from "../types/consolidacion";

// Encapsula el consumo de ConsolidacionesController (backend, docs/05-api.md §31). Exige JWT +
// PEDIDO_VER (listar/obtener) o PEDIDO_CONSOLIDAR (crear) — RN-064/RN-063, apiClient.ts adjunta
// el token automáticamente. Solo cubre lo que el backend expone: sin PUT/DELETE ni
// "{id}/generar" (sin caso de uso que los respalde).
export const consolidacionesService = {
  listar: (periodoId?: number | null): Promise<Consolidacion[]> =>
    apiClient.get<Consolidacion[]>(periodoId != null ? `/api/v1/consolidaciones?periodoId=${periodoId}` : "/api/v1/consolidaciones"),

  obtener: (consolidacionId: number): Promise<Consolidacion> =>
    apiClient.get<Consolidacion>(`/api/v1/consolidaciones/${consolidacionId}`),

  crear: (request: CrearConsolidacionRequest): Promise<Consolidacion> =>
    apiClient.post<Consolidacion>("/api/v1/consolidaciones", request),
};
