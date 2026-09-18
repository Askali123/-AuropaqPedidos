import { apiClient } from "../services/apiClient";
import type { LoginRequest, LoginResponse, Permiso } from "../types/auth";

// Encapsula el consumo de AuthController (backend, docs/05-api.md §56.1/§56.3).
export const authService = {
  login: (request: LoginRequest): Promise<LoginResponse> =>
    apiClient.post<LoginResponse>("/api/v1/auth/login", request),

  // TASK-102: autoconsulta de permisos reales del usuario autenticado — base para filtrar
  // navegación/rutas (AuthContext.tsx). Sin permiso específico requerido, solo sesión activa.
  misPermisos: (): Promise<Permiso[]> => apiClient.get<Permiso[]>("/api/v1/auth/mis-permisos"),
};
