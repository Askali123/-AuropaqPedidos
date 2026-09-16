import { apiClient } from "../services/apiClient";
import type { LoginRequest, LoginResponse } from "../types/auth";

// Encapsula el consumo de AuthController (backend, docs/05-api.md §56.1). Único endpoint público
// de sesión — el resto del módulo auth/ vive de su respuesta.
export const authService = {
  login: (request: LoginRequest): Promise<LoginResponse> =>
    apiClient.post<LoginResponse>("/api/v1/auth/login", request),
};
