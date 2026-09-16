import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { seguridadService } from "../../services/seguridadService";
import type { Permiso } from "../../types/seguridad";

interface RolPermisosPanelProps {
  rolId: number;
  todosLosPermisos: Permiso[];
}

// docs/05-api.md §63. Mismo patrón que UsuarioRolesPanel: consulta los permisos ya asignados
// (§63.2) y permite asignar uno nuevo (§63.1). Sin "quitar permiso" (§63.3, no existe en el
// backend).
export function RolPermisosPanel({ rolId, todosLosPermisos }: RolPermisosPanelProps) {
  const [version, setVersion] = useState(0);
  const permisos = useConsultaLista<Permiso>(() => seguridadService.listarPermisosDeRol(rolId), [rolId, version]);

  const [permisoAAsignar, setPermisoAAsignar] = useState("");
  const [asignando, setAsignando] = useState(false);
  const [errorAsignar, setErrorAsignar] = useState<ApiRequestError | null>(null);

  async function asignarPermiso() {
    if (!permisoAAsignar) return;
    setAsignando(true);
    setErrorAsignar(null);
    try {
      await seguridadService.asignarPermisoARol(rolId, Number(permisoAAsignar));
      setPermisoAAsignar("");
      setVersion((v) => v + 1);
    } catch (error) {
      setErrorAsignar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAsignando(false);
    }
  }

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <h4 className="text-xs font-semibold text-slate-600">Permisos asignados</h4>

      {permisos.cargando ? (
        <p className="mt-1 text-sm text-slate-500">Cargando...</p>
      ) : permisos.error ? (
        <div className="mt-1">
          <ErrorBanner error={permisos.error} />
        </div>
      ) : permisos.datos.length === 0 ? (
        <p className="mt-1 text-sm text-slate-500">Sin permisos asignados.</p>
      ) : (
        <ul className="mt-1 flex flex-wrap gap-1.5">
          {permisos.datos.map((permiso) => (
            <li
              key={permiso.id}
              title={permiso.descripcion ?? undefined}
              className="rounded-full bg-emerald-100 px-2.5 py-0.5 text-xs font-medium text-emerald-800"
            >
              {permiso.codigo}
            </li>
          ))}
        </ul>
      )}

      <div className="mt-3 flex items-center gap-2">
        <select
          value={permisoAAsignar}
          onChange={(evento) => setPermisoAAsignar(evento.target.value)}
          className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        >
          <option value="">Asignar permiso...</option>
          {todosLosPermisos.map((permiso) => (
            <option key={permiso.id} value={permiso.id}>
              {permiso.codigo} — {permiso.nombre}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={asignarPermiso}
          disabled={!permisoAAsignar || asignando}
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
