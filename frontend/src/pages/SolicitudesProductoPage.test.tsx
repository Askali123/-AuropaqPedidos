import { afterEach, describe, expect, it, vi } from "vitest";
import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { SolicitudesProductoPage } from "./SolicitudesProductoPage";
import { solicitudesProductoService } from "../services/solicitudesProductoService";
import * as AuthContextModule from "../auth/AuthContext";
import type { SolicitudProductoCatalogo } from "../types/solicitudesProducto";

// TASK-103 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F2). Antes de esta
// tarea, la pantalla mostraba el formulario "Solicitar producto" y la bandeja de pendientes con
// "Resolver" a cualquier usuario por igual, sin importar sus permisos reales — un Solicitante
// veía "Resolver" y fallaba con 403 recién al hacer clic. Estas pruebas cubren exactamente los 3
// permisos reales que conviven en la pantalla (docs/06-seguridad.md §52): PRODUCTO_SOLICITAR,
// PRODUCTO_VER, PRODUCTO_CREAR — no son "mitad y mitad", un Solicitante sí tiene PRODUCTO_VER.
vi.mock("../auth/AuthContext", async () => {
  const real = await vi.importActual<typeof AuthContextModule>("../auth/AuthContext");
  return { ...real, useAuth: vi.fn() };
});
vi.mock("../services/solicitudesProductoService");

const useAuthMock = vi.mocked(AuthContextModule.useAuth);

const solicitudDePrueba: SolicitudProductoCatalogo = {
  id: 1,
  empresaId: 1,
  usuarioId: 55,
  nombreSolicitado: "Guante de seguridad talla M",
  descripcion: null,
  observacion: null,
  estado: "Pendiente",
  productoResultanteId: null,
  fechaSolicitud: "2026-09-18T00:00:00Z",
  fechaResolucion: null,
  usuarioResolucionId: null,
  motivoResolucion: null,
};

function mockearPermisos(permisos: string[]) {
  useAuthMock.mockReturnValue({
    usuario: null,
    iniciandoSesion: false,
    errorLogin: null,
    login: vi.fn(),
    logout: vi.fn(),
    permisos,
    cargandoPermisos: false,
    tieneAlgunPermiso: (...codigos: string[]) => codigos.some((codigo) => permisos.includes(codigo)),
  });
}

describe("SolicitudesProductoPage", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("un Solicitante puro ve el formulario y la bandeja, pero no el botón Resolver", async () => {
    mockearPermisos(["PRODUCTO_SOLICITAR", "PRODUCTO_VER"]);
    vi.mocked(solicitudesProductoService.listarPendientes).mockResolvedValue([solicitudDePrueba]);

    render(<SolicitudesProductoPage />);

    expect(screen.getByText("Solicitar producto")).toBeInTheDocument();
    await waitFor(() => expect(screen.getByText("Guante de seguridad talla M")).toBeInTheDocument());
    expect(screen.queryByText("Resolver")).not.toBeInTheDocument();
    expect(solicitudesProductoService.listarPendientes).toHaveBeenCalled();
  });

  it("un Gestor_Requisiciones ve la bandeja con Resolver, pero no el formulario de solicitar", async () => {
    mockearPermisos(["PRODUCTO_VER", "PRODUCTO_CREAR"]);
    vi.mocked(solicitudesProductoService.listarPendientes).mockResolvedValue([solicitudDePrueba]);

    render(<SolicitudesProductoPage />);

    expect(screen.queryByText("Solicitar producto")).not.toBeInTheDocument();
    await waitFor(() => expect(screen.getByText("Resolver")).toBeInTheDocument());
  });

  it("sin PRODUCTO_VER no consulta la bandeja de pendientes (evita un 403 innecesario)", () => {
    mockearPermisos(["PRODUCTO_SOLICITAR"]);

    render(<SolicitudesProductoPage />);

    expect(screen.getByText("Solicitar producto")).toBeInTheDocument();
    expect(solicitudesProductoService.listarPendientes).not.toHaveBeenCalled();
  });
});
