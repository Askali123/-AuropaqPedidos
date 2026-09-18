import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { authService } from "./authService";
import { guardarSesion, limpiarSesion, obtenerUsuario, suscribirCambiosDeSesion } from "./session";
import { ApiRequestError } from "../services/apiClient";
import type { UsuarioAutenticado } from "../types/auth";

interface AuthContextValue {
  usuario: UsuarioAutenticado | null;
  iniciandoSesion: boolean;
  errorLogin: ApiRequestError | null;
  login: (correo: string, password: string) => Promise<void>;
  logout: () => void;
  // TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md): códigos de permiso reales
  // del usuario autenticado, para filtrar navegación/rutas. cargandoPermisos evita que un
  // guard de ruta deniegue por un instante mientras todavía no llegó la respuesta.
  permisos: string[];
  cargandoPermisos: boolean;
  tieneAlgunPermiso: (...codigos: string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

// Fuente única de la sesión para toda la app: envuelve session.ts (localStorage) en estado de
// React. Se suscribe a suscribirCambiosDeSesion para reaccionar también cuando apiClient limpia
// la sesión por su cuenta (401 con token ya guardado, ver services/apiClient.ts) — sin esto, la
// UI seguiría mostrando al usuario autenticado aunque el token ya no sirva.
export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioAutenticado | null>(() => obtenerUsuario());
  const [iniciandoSesion, setIniciandoSesion] = useState(false);
  const [errorLogin, setErrorLogin] = useState<ApiRequestError | null>(null);
  const [permisos, setPermisos] = useState<string[]>([]);
  const [cargandoPermisos, setCargandoPermisos] = useState(true);

  useEffect(() => suscribirCambiosDeSesion(() => setUsuario(obtenerUsuario())), []);

  // Se consulta cada vez que cambia la sesión (login, logout, o limpieza automática por 401):
  // fuente de verdad en vivo (mismo criterio que ADR-058 para la autorización del backend), no
  // una copia que pueda quedar desactualizada. Sin sesión, no hay nada que consultar — permisos
  // queda vacío (denegación por defecto, CLAUDE.md §67).
  useEffect(() => {
    if (!usuario) {
      setPermisos([]);
      setCargandoPermisos(false);
      return;
    }

    let cancelado = false;
    setCargandoPermisos(true);
    authService
      .misPermisos()
      .then((respuesta) => {
        if (!cancelado) setPermisos(respuesta.map((permiso) => permiso.codigo));
      })
      .catch(() => {
        if (!cancelado) setPermisos([]);
      })
      .finally(() => {
        if (!cancelado) setCargandoPermisos(false);
      });

    return () => {
      cancelado = true;
    };
  }, [usuario]);

  async function login(correo: string, password: string) {
    setIniciandoSesion(true);
    setErrorLogin(null);
    try {
      const respuesta = await authService.login({ correo, password });
      guardarSesion(respuesta.token, respuesta.usuario);
    } catch (error) {
      setErrorLogin(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      throw error;
    } finally {
      setIniciandoSesion(false);
    }
  }

  function logout() {
    limpiarSesion();
  }

  function tieneAlgunPermiso(...codigos: string[]): boolean {
    return codigos.some((codigo) => permisos.includes(codigo));
  }

  return (
    <AuthContext.Provider
      value={{ usuario, iniciandoSesion, errorLogin, login, logout, permisos, cargandoPermisos, tieneAlgunPermiso }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error("useAuth debe usarse dentro de <AuthProvider>.");
  return contexto;
}
