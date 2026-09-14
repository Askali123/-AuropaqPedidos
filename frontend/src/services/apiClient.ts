import { API_BASE_URL } from "../config/api";
import type { ApiErrorResponse } from "../types/api";

// Error tipado a partir del envoltorio { error: { code, message, details } } que devuelve
// la Api (Api/ErrorHandling/ExcepcionesDeNegocioHandler.cs, backend). Permite a los componentes
// distinguir 404/422/etc. sin volver a parsear la respuesta.
export class ApiRequestError extends Error {
  readonly status: number;
  readonly code: string;
  readonly details: string[];

  constructor(status: number, code: string, message: string, details: string[] = []) {
    super(message);
    this.name = "ApiRequestError";
    this.status = status;
    this.code = code;
    this.details = details;
  }
}

async function request<TResponse>(path: string, options: RequestInit): Promise<TResponse> {
  let respuesta: Response;

  try {
    respuesta = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    });
  } catch {
    // No es un error de la Api (400/404/422/500 con envoltorio): la Api no respondió en
    // absoluto (servidor caído, CORS bloqueado por el navegador, red).
    throw new ApiRequestError(0, "SIN_CONEXION", "No fue posible contactar la Api. Verifica que esté en ejecución.");
  }

  const cuerpo = await respuesta.json().catch(() => null);

  if (!respuesta.ok) {
    const error = (cuerpo as ApiErrorResponse | null)?.error;
    throw new ApiRequestError(
      respuesta.status,
      error?.code ?? "ERROR_DESCONOCIDO",
      error?.message ?? `La Api respondió con estado ${respuesta.status}.`,
      error?.details ?? [],
    );
  }

  return (cuerpo as { data: TResponse }).data;
}

export const apiClient = {
  get: <TResponse>(path: string, headers?: Record<string, string>): Promise<TResponse> =>
    request<TResponse>(path, { method: "GET", headers }),

  // body es opcional: POST /requisiciones/{id}/guardar (docs/05-api.md §54.4, fila 8) no recibe
  // cuerpo alguno.
  post: <TResponse>(path: string, body?: unknown, headers?: Record<string, string>): Promise<TResponse> =>
    request<TResponse>(path, {
      method: "POST",
      body: body === undefined ? undefined : JSON.stringify(body),
      headers,
    }),

  put: <TResponse>(path: string, body: unknown, headers?: Record<string, string>): Promise<TResponse> =>
    request<TResponse>(path, { method: "PUT", body: JSON.stringify(body), headers }),

  delete: <TResponse>(path: string, headers?: Record<string, string>): Promise<TResponse> =>
    request<TResponse>(path, { method: "DELETE", headers }),
};
