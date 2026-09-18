import { afterEach, describe, expect, it, vi } from "vitest";
import { cleanup, render, screen } from "@testing-library/react";
import { RutaConPermiso } from "./RutaConPermiso";
import * as AuthContextModule from "../auth/AuthContext";

// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md). useAuth se mockea
// directamente (en vez de envolver en un <AuthProvider> real) para probar el guard de forma
// aislada de la carga real de sesión/permisos — ver AppLayout.test.tsx para la integración con
// el menú completo.
vi.mock("../auth/AuthContext", async () => {
  const real = await vi.importActual<typeof AuthContextModule>("../auth/AuthContext");
  return { ...real, useAuth: vi.fn() };
});

const useAuthMock = vi.mocked(AuthContextModule.useAuth);

function mockearAuth(permisos: string[], cargandoPermisos: boolean) {
  useAuthMock.mockReturnValue({
    usuario: null,
    iniciandoSesion: false,
    errorLogin: null,
    login: vi.fn(),
    logout: vi.fn(),
    permisos,
    cargandoPermisos,
    tieneAlgunPermiso: (...codigos: string[]) => codigos.some((codigo) => permisos.includes(codigo)),
  });
}

describe("RutaConPermiso", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("muestra un estado de carga mientras los permisos todavía no llegan", () => {
    mockearAuth([], true);

    render(
      <RutaConPermiso permisos={["REQUISICION_VER"]}>
        <p>Contenido protegido</p>
      </RutaConPermiso>,
    );

    expect(screen.getByText("Cargando...")).toBeInTheDocument();
    expect(screen.queryByText("Contenido protegido")).not.toBeInTheDocument();
  });

  it("muestra el contenido cuando el usuario tiene alguno de los permisos requeridos", () => {
    mockearAuth(["REQUISICION_VER"], false);

    render(
      <RutaConPermiso permisos={["REQUISICION_CREAR", "REQUISICION_VER"]}>
        <p>Contenido protegido</p>
      </RutaConPermiso>,
    );

    expect(screen.getByText("Contenido protegido")).toBeInTheDocument();
  });

  it('muestra "Sin acceso" cuando el usuario no tiene ninguno de los permisos requeridos', () => {
    mockearAuth(["REQUISICION_VER"], false);

    render(
      <RutaConPermiso permisos={["SEGURIDAD_VER", "SEGURIDAD_ADMINISTRAR"]}>
        <p>Contenido protegido</p>
      </RutaConPermiso>,
    );

    expect(screen.getByText("Sin acceso")).toBeInTheDocument();
    expect(screen.queryByText("Contenido protegido")).not.toBeInTheDocument();
  });
});
