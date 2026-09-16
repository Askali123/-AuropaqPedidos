import { Fragment, useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { RolPermisosPanel } from "./RolPermisosPanel";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { seguridadService } from "../../services/seguridadService";
import type { Permiso } from "../../types/seguridad";

// docs/05-api.md §59/§63. Lista de Roles (§59.1, GLOBAL, sin EmpresaId) + alta (§59.2) + panel
// expandible por fila con sus permisos (§63). Sin PUT/DELETE/activar-desactivar (§63.3, no
// existe en el backend).
export function RolesSeccion() {
  const [version, setVersion] = useState(0);
  const roles = useConsultaLista(() => seguridadService.listarRoles(), [version]);
  const permisos = useConsultaLista<Permiso>(() => seguridadService.listarPermisos(), []);

  const [expandidoId, setExpandidoId] = useState<number | null>(null);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  async function crearRol(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await seguridadService.crearRol({ nombre, descripcion: descripcion || undefined });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setNombre("");
      setDescripcion("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-slate-800">Roles</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nuevo rol"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearRol} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Nombre</label>
              <input
                required
                value={nombre}
                onChange={(evento) => setNombre(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Descripción (opcional)</label>
              <input
                value={descripcion}
                onChange={(evento) => setDescripcion(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={creando}
            className="mt-3 rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Creando..." : "Crear rol"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {roles.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : roles.error ? (
          <div className="p-4">
            <ErrorBanner error={roles.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Descripción</th>
                <th className="px-4 py-2 font-medium">Activo</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {roles.datos.map((rol) => (
                <Fragment key={rol.id}>
                  <tr className="border-b border-slate-100">
                    <td className="px-4 py-2 text-slate-900">{rol.id}</td>
                    <td className="px-4 py-2 text-slate-900">{rol.nombre}</td>
                    <td className="px-4 py-2 text-slate-900">{rol.descripcion ?? "—"}</td>
                    <td className="px-4 py-2 text-slate-900">{rol.activo ? "Sí" : "No"}</td>
                    <td className="px-4 py-2">
                      <button
                        type="button"
                        onClick={() => setExpandidoId(expandidoId === rol.id ? null : rol.id)}
                        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                      >
                        {expandidoId === rol.id ? "Ocultar permisos" : "Ver permisos"}
                      </button>
                    </td>
                  </tr>
                  {expandidoId === rol.id && (
                    <tr>
                      <td colSpan={5} className="bg-slate-50 px-4 py-3">
                        <RolPermisosPanel rolId={rol.id} todosLosPermisos={permisos.datos} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {roles.datos.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-3 text-sm text-slate-500">
                    No hay roles registrados.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
