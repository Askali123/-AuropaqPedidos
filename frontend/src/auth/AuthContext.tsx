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

  useEffect(() => suscribirCambiosDeSesion(() => setUsuario(obtenerUsuario())), []);

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

  return (
    <AuthContext.Provider value={{ usuario, iniciandoSesion, errorLogin, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error("useAuth debe usarse dentro de <AuthProvider>.");
  return contexto;
}
