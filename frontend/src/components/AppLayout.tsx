import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ENLACES } from "../routes/enlaces";

// Reemplaza el selector de vista manual de App.tsx (useState<Vista>) — ya no se justifica con
// más de dos pantallas (Requisiciones/Facturación ya tenían ese comentario explícito).
//
// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F1): la barra de
// navegación ahora sí oculta los enlaces que el usuario no puede usar, usando los permisos
// reales obtenidos vía GET /auth/mis-permisos (AuthContext.tieneAlgunPermiso) — antes mostraba
// las 9 secciones a cualquier usuario autenticado por igual. Mientras cargandoPermisos es true
// no se oculta nada todavía (evita un parpadeo de "sin acceso" antes de que llegue la
// respuesta) — RutaConPermiso (AppRoutes.tsx) protege la ruta en sí para ese instante.
export function AppLayout() {
  const { usuario, logout, cargandoPermisos, tieneAlgunPermiso } = useAuth();
  const enlacesVisibles = cargandoPermisos ? ENLACES : ENLACES.filter((enlace) => tieneAlgunPermiso(...enlace.permisos));

  return (
    <div className="min-h-screen bg-slate-100">
      <nav className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-200 bg-white px-4 py-2">
        <div className="flex flex-wrap gap-2">
          {enlacesVisibles.map((enlace) => (
            <NavLink
              key={enlace.ruta}
              to={enlace.ruta}
              className={({ isActive }) =>
                `rounded px-3 py-1.5 text-sm font-medium ${
                  isActive ? "bg-blue-600 text-white" : "text-slate-600 hover:bg-slate-100"
                }`
              }
            >
              {enlace.etiqueta}
            </NavLink>
          ))}
        </div>

        <div className="flex shrink-0 items-center gap-3 text-sm text-slate-600">
          <span>
            {usuario?.nombre} {usuario?.apellido ?? ""}
          </span>
          <button type="button" onClick={logout} className="rounded px-2 py-1 font-medium text-slate-500 hover:bg-slate-100">
            Salir
          </button>
        </div>
      </nav>

      <Outlet />
    </div>
  );
}
