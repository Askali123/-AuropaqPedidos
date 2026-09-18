import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient, ApiRequestError } from "./apiClient";
import * as sesion from "../auth/session";
import type { UsuarioAutenticado } from "../types/auth";

const usuarioDePrueba: UsuarioAutenticado = {
  id: 1,
  empresaId: 1,
  nombre: "Ana",
  apellido: "Perez",
  correo: "ana@auropaq.dev",
  activo: true,
  fechaCreacion: "2026-01-01T00:00:00Z",
  fechaActualizacion: "2026-01-01T00:00:00Z",
};

// Cubre la lógica real de apiClient.ts: adjuntar el JWT, traducir el envoltorio de error de la
// Api a ApiRequestError, limpiar la sesión solo en el caso correcto de 401, y distinguir un 401
// del backend de una Api inalcanzable (CLAUDE.md §45: "Frontend → Prueba correspondiente").

function respuestaJson(status: number, cuerpo: unknown): Response {
  return new Response(JSON.stringify(cuerpo), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("apiClient", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.stubGlobal("fetch", vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("desenvuelve el campo data de una respuesta exitosa", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(respuestaJson(200, { data: { id: 7 } }));

    const resultado = await apiClient.get<{ id: number }>("/api/v1/algo");

    expect(resultado).toEqual({ id: 7 });
  });

  it("no adjunta Authorization cuando no hay sesión guardada", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(respuestaJson(200, { data: null }));

    await apiClient.get("/api/v1/algo");

    const [, opciones] = vi.mocked(fetch).mock.calls[0]!;
    const headers = opciones!.headers as Record<string, string>;
    expect(headers.Authorization).toBeUndefined();
  });

  it("adjunta el token guardado como Bearer", async () => {
    sesion.guardarSesion("token-de-prueba", usuarioDePrueba);
    vi.mocked(fetch).mockResolvedValueOnce(respuestaJson(200, { data: null }));

    await apiClient.get("/api/v1/algo");

    const [, opciones] = vi.mocked(fetch).mock.calls[0]!;
    const headers = opciones!.headers as Record<string, string>;
    expect(headers.Authorization).toBe("Bearer token-de-prueba");
  });

  it("traduce un error de negocio (422) al envoltorio de la Api", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      respuestaJson(422, {
        error: { code: "REGLA_DE_NEGOCIO_VIOLADA", message: "No se puede enviar.", details: ["motivo"] },
      }),
    );

    await expect(apiClient.post("/api/v1/algo", {})).rejects.toMatchObject({
      status: 422,
      code: "REGLA_DE_NEGOCIO_VIOLADA",
      message: "No se puede enviar.",
      details: ["motivo"],
    });
  });

  it("limpia la sesión cuando un 401 llega con un token ya guardado (sesión expirada)", async () => {
    sesion.guardarSesion("token-viejo", usuarioDePrueba);
    vi.mocked(fetch).mockResolvedValueOnce(
      respuestaJson(401, { error: { code: "NO_AUTENTICADO", message: "Sesión expirada.", details: [] } }),
    );

    await expect(apiClient.get("/api/v1/algo")).rejects.toBeInstanceOf(ApiRequestError);

    expect(sesion.obtenerToken()).toBeNull();
  });

  it("NO limpia sesión en un 401 sin token previo (p. ej. login con credenciales inválidas)", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      respuestaJson(401, { error: { code: "CREDENCIALES_INVALIDAS", message: "Correo o contraseña incorrectos.", details: [] } }),
    );

    await expect(apiClient.post("/api/v1/auth/login", {})).rejects.toMatchObject({
      status: 401,
      code: "CREDENCIALES_INVALIDAS",
    });
    expect(sesion.obtenerToken()).toBeNull();
  });

  it("traduce un fallo de red (Api inalcanzable) a SIN_CONEXION", async () => {
    vi.mocked(fetch).mockRejectedValueOnce(new TypeError("Failed to fetch"));

    await expect(apiClient.get("/api/v1/algo")).rejects.toMatchObject({
      status: 0,
      code: "SIN_CONEXION",
    });
  });
});
