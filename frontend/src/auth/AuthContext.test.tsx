import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthProvider, useAuth } from "./AuthContext";
import { authService } from "./authService";
import type { Permiso, UsuarioAutenticado } from "../types/auth";

// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md). Cubre la lógica real del
// contexto: que los permisos se consultan (GET /auth/mis-permisos) recién después de iniciar
// sesión, y que un fallo de esa consulta no deja al usuario con permisos "por defecto" — queda
// en cero (denegación por defecto, CLAUDE.md §67), no en un estado sin definir.
vi.mock("./authService");

const usuarioDePrueba: UsuarioAutenticado = {
  id: 1,
  empresaId: 1,
  nombre: "Ana",
  apellido: null,
  correo: "ana@auropaq.dev",
  activo: true,
  fechaCreacion: "2026-01-01T00:00:00Z",
  fechaActualizacion: "2026-01-01T00:00:00Z",
};

function Consumidor() {
  const { usuario, cargandoPermisos, permisos, tieneAlgunPermiso, login } = useAuth();
  return (
    <div>
      <p>usuario: {usuario?.correo ?? "ninguno"}</p>
      <p>cargando: {String(cargandoPermisos)}</p>
      <p>permisos: {permisos.join(",")}</p>
      <p>tienePedidoVer: {String(tieneAlgunPermiso("PEDIDO_VER"))}</p>
      <button onClick={() => login("ana@auropaq.dev", "clave-de-prueba")}>Entrar</button>
    </div>
  );
}

describe("AuthContext — permisos", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("consulta los permisos reales después de iniciar sesión", async () => {
    const permisosDeLaApi: Permiso[] = [{ id: 1, codigo: "PEDIDO_VER", nombre: "Ver pedidos", descripcion: null }];
    vi.mocked(authService.login).mockResolvedValue({
      token: "token-de-prueba",
      fechaExpiracion: "2026-01-01T00:00:00Z",
      usuario: usuarioDePrueba,
    });
    vi.mocked(authService.misPermisos).mockResolvedValue(permisosDeLaApi);

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );
    await userEvent.click(screen.getByText("Entrar"));

    await waitFor(() => expect(screen.getByText("usuario: ana@auropaq.dev")).toBeInTheDocument());
    await waitFor(() => expect(screen.getByText("cargando: false")).toBeInTheDocument());
    expect(screen.getByText("permisos: PEDIDO_VER")).toBeInTheDocument();
    expect(screen.getByText("tienePedidoVer: true")).toBeInTheDocument();
  });

  it("deja los permisos en una lista vacía si la consulta falla (denegación por defecto)", async () => {
    vi.mocked(authService.login).mockResolvedValue({
      token: "token-de-prueba",
      fechaExpiracion: "2026-01-01T00:00:00Z",
      usuario: usuarioDePrueba,
    });
    vi.mocked(authService.misPermisos).mockRejectedValue(new Error("La Api no respondió."));

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );
    await userEvent.click(screen.getByText("Entrar"));

    await waitFor(() => expect(screen.getByText("cargando: false")).toBeInTheDocument());
    expect(screen.getByText("permisos:")).toBeInTheDocument();
    expect(screen.getByText("tienePedidoVer: false")).toBeInTheDocument();
  });
});
