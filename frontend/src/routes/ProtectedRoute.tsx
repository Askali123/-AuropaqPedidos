import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

// Redirige a /login preservando la ruta de origen (state.desde) para volver ahí después de
// iniciar sesión — ver LoginPage. El backend sigue siendo quien decide 401/403 en cada endpoint
// (03-arquitectura.md §25/§997): esto solo evita mostrar una pantalla que fallaría de entrada
// porque no hay ningún token que enviar.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { usuario } = useAuth();
  const location = useLocation();

  if (!usuario) {
    return <Navigate to="/login" replace state={{ desde: location.pathname }} />;
  }

  return <>{children}</>;
}
