import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

// Reemplaza el selector de vista manual de App.tsx (useState<Vista>) — ya no se justifica con
// más de dos pantallas (Requisiciones/Facturación ya tenían ese comentario explícito). La barra
// de navegación no oculta ningún enlace por permiso todavía: el JWT no trae claims de
// roles/permisos (docs/05-api.md §56), así que "adaptar la interfaz según permisos conocidos"
// (03-arquitectura.md §997) queda para cuando exista ese dato — por ahora el backend sigue
// siendo quien rechaza con 403 lo que el usuario no puede hacer.
const ENLACES = [
  { a: "/catalogo", etiqueta: "Catálogo" },
  { a: "/requisiciones", etiqueta: "Requisiciones" },
  { a: "/revision", etiqueta: "Revisión" },
  { a: "/solicitudes-producto", etiqueta: "Solicitudes de producto" },
  { a: "/consolidacion", etiqueta: "Consolidación" },
  { a: "/pedidos-proveedor", etiqueta: "Pedidos a proveedor" },
  { a: "/entregas", etiqueta: "Entregas" },
  { a: "/facturacion", etiqueta: "Facturación" },
  { a: "/seguridad", etiqueta: "Seguridad" },
] as const;

export function AppLayout() {
  const { usuario, logout } = useAuth();

  return (
    <div className="min-h-screen bg-slate-100">
      <nav className="flex items-center justify-between gap-2 border-b border-slate-200 bg-white px-4 py-2">
        <div className="flex gap-2">
          {ENLACES.map((enlace) => (
            <NavLink
              key={enlace.a}
              to={enlace.a}
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

        <div className="flex items-center gap-3 text-sm text-slate-600">
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
