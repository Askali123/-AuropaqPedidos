import { afterEach, describe, expect, it, vi } from "vitest";
import { cleanup, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { AppLayout } from "./AppLayout";
import { ENLACES } from "../routes/enlaces";
import * as AuthContextModule from "../auth/AuthContext";

// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F1). Cubre
// explícitamente el criterio de aceptación del audit: "un Solicitante ve solo Requisiciones,
// Solicitudes de producto y Catálogo; un Administrador ve las 9 secciones".
vi.mock("../auth/AuthContext", async () => {
  const real = await vi.importActual<typeof AuthContextModule>("../auth/AuthContext");
  return { ...real, useAuth: vi.fn() };
});

const useAuthMock = vi.mocked(AuthContextModule.useAuth);

const usuarioDePrueba = {
  id: 1,
  empresaId: 1,
  nombre: "Ana",
  apellido: null,
  correo: "ana@auropaq.dev",
  activo: true,
  fechaCreacion: "2026-01-01T00:00:00Z",
  fechaActualizacion: "2026-01-01T00:00:00Z",
};

function renderizarLayout(permisos: string[]) {
  useAuthMock.mockReturnValue({
    usuario: usuarioDePrueba,
    iniciandoSesion: false,
    errorLogin: null,
    login: vi.fn(),
    logout: vi.fn(),
    permisos,
    cargandoPermisos: false,
    tieneAlgunPermiso: (...codigos: string[]) => codigos.some((codigo) => permisos.includes(codigo)),
  });

  return render(
    <MemoryRouter initialEntries={["/requisiciones"]}>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/requisiciones" element={<p>Página de requisiciones</p>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe("AppLayout", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("un Solicitante ve solo Requisiciones, Solicitudes de producto y Catálogo", () => {
    // Permisos reales sembrados para el rol Solicitante (docs/06-seguridad.md §53).
    renderizarLayout([
      "REQUISICION_CREAR",
      "REQUISICION_VER",
      "REQUISICION_MODIFICAR",
      "REQUISICION_ENVIAR",
      "PRODUCTO_VER",
      "PRODUCTO_SOLICITAR",
      "PERIODO_VER",
      "ORGANIZACION_VER",
    ]);

    expect(screen.getByRole("link", { name: "Requisiciones" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Solicitudes de producto" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Catálogo" })).toBeInTheDocument();

    expect(screen.queryByRole("link", { name: "Revisión" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Consolidación" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Pedidos a proveedor" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Entregas" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Facturación" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Seguridad" })).not.toBeInTheDocument();
  });

  it("un Administrador ve las 9 secciones", () => {
    const todosLosPermisos = ENLACES.flatMap((enlace) => enlace.permisos);

    renderizarLayout(todosLosPermisos);

    for (const enlace of ENLACES) {
      expect(screen.getByRole("link", { name: enlace.etiqueta })).toBeInTheDocument();
    }
  });

  it("un Gestor_Requisiciones ve Requisiciones y Revisión, pero no Seguridad ni Facturación", () => {
    renderizarLayout([
      "REQUISICION_CREAR",
      "REQUISICION_VER",
      "REQUISICION_MODIFICAR",
      "REQUISICION_ENVIAR",
      "REQUISICION_APROBAR",
      "REQUISICION_DEVOLVER",
      "PRODUCTO_VER",
      "PRODUCTO_CREAR",
      "PRODUCTO_EDITAR",
      "PERIODO_VER",
      "ORGANIZACION_VER",
    ]);

    expect(screen.getByRole("link", { name: "Requisiciones" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Revisión" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Seguridad" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Facturación" })).not.toBeInTheDocument();
  });
});
