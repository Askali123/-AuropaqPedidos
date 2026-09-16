import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { seguridadService } from "../../services/seguridadService";
import type { Rol } from "../../types/seguridad";

interface UsuarioRolesPanelProps {
  usuarioId: number;
  todosLosRoles: Rol[];
}

// docs/05-api.md §61. Panel expandible por fila de UsuariosSeccion: consulta los roles ya
// asignados (§61.2) y permite asignar uno nuevo de la lista global de Roles (§61.1). Sin "quitar
// rol": el endpoint no existe (05-api.md §63.3, decisión ya tomada, no se inventa aquí). RN-060
// punto 7 (prevención de auto-escalamiento) la valida el backend — el formulario no repite esa
// regla, solo muestra el error si el backend la rechaza.
export function UsuarioRolesPanel({ usuarioId, todosLosRoles }: UsuarioRolesPanelProps) {
  const [version, setVersion] = useState(0);
  const roles = useConsultaLista<Rol>(() => seguridadService.listarRolesDeUsuario(usuarioId), [usuarioId, version]);

  const [rolAAsignar, setRolAAsignar] = useState("");
  const [asignando, setAsignando] = useState(false);
  const [errorAsignar, setErrorAsignar] = useState<ApiRequestError | null>(null);

  async function asignarRol() {
    if (!rolAAsignar) return;
    setAsignando(true);
    setErrorAsignar(null);
    try {
      await seguridadService.asignarRolAUsuario(usuarioId, Number(rolAAsignar));
      setRolAAsignar("");
      setVersion((v) => v + 1);
    } catch (error) {
      setErrorAsignar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAsignando(false);
    }
  }

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <h4 className="text-xs font-semibold text-slate-600">Roles asignados</h4>

      {roles.cargando ? (
        <p className="mt-1 text-sm text-slate-500">Cargando...</p>
      ) : roles.error ? (
        <div className="mt-1">
          <ErrorBanner error={roles.error} />
        </div>
      ) : roles.datos.length === 0 ? (
        <p className="mt-1 text-sm text-slate-500">Sin roles asignados.</p>
      ) : (
        <ul className="mt-1 flex flex-wrap gap-1.5">
          {roles.datos.map((rol) => (
            <li key={rol.id} className="rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800">
              {rol.nombre}
            </li>
          ))}
        </ul>
      )}

      <div className="mt-3 flex items-center gap-2">
        <select
          value={rolAAsignar}
          onChange={(evento) => setRolAAsignar(evento.target.value)}
          className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        >
          <option value="">Asignar rol...</option>
          {todosLosRoles.map((rol) => (
            <option key={rol.id} value={rol.id}>
              {rol.nombre}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={asignarRol}
          disabled={!rolAAsignar || asignando}
          className="rounded bg-slate-700 px-3 py-1 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {asignando ? "Asignando..." : "Asignar"}
        </button>
      </div>

      {errorAsignar && (
        <div className="mt-2">
          <ErrorBanner error={errorAsignar} />
        </div>
      )}
    </div>
  );
}
