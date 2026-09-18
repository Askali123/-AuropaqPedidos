import type { ReactNode } from "react";
import { useAuth } from "../auth/AuthContext";

interface RutaConPermisoProps {
  permisos: readonly string[];
  children: ReactNode;
}

// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F1): protección de
// rutas por permiso real, además del filtrado del menú (AppLayout.tsx) — cubre el caso de que
// alguien navegue directamente a la URL sin pasar por el menú. El backend sigue siendo la única
// protección real (CLAUDE.md §13/§14): esto es UX, evita mostrar un formulario que fallaría con
// 403 recién al hacer clic, no un mecanismo de seguridad en sí mismo.
export function RutaConPermiso({ permisos, children }: RutaConPermisoProps) {
  const { cargandoPermisos, tieneAlgunPermiso } = useAuth();

  if (cargandoPermisos) {
    return <p className="p-8 text-sm text-slate-500">Cargando...</p>;
  }

  if (!tieneAlgunPermiso(...permisos)) {
    return (
      <main className="mx-auto max-w-3xl px-4 py-8">
        <h1 className="text-2xl font-semibold text-slate-900">Sin acceso</h1>
        <p className="mt-2 text-sm text-slate-500">No tienes permiso para ver esta pantalla.</p>
      </main>
    );
  }

  return <>{children}</>;
}
