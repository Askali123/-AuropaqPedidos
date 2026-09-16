import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { seguridadService } from "../../services/seguridadService";

// docs/05-api.md §60. Lista de Permisos (§60.1, GLOBAL) + alta (§60.2). Sin PUT/DELETE (§63.3).
// "Asignar a un rol" vive en RolesSeccion (RolPermisosPanel), no aquí: el permiso no conoce a
// qué roles pertenece hasta que se le asigna uno.
export function PermisosSeccion() {
  const [version, setVersion] = useState(0);
  const permisos = useConsultaLista(() => seguridadService.listarPermisos(), [version]);

  const [formAbierto, setFormAbierto] = useState(false);
  const [codigo, setCodigo] = useState("");
  const [nombre, setNombre] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  async function crearPermiso(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await seguridadService.crearPermiso({ codigo, nombre, descripcion: descripcion || undefined });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setCodigo("");
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
        <h2 className="text-base font-semibold text-slate-800">Permisos</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nuevo permiso"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearPermiso} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
          <div className="grid gap-3 sm:grid-cols-3">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Código</label>
              <input
                required
                placeholder="MODULO_ACCION"
                value={codigo}
                onChange={(evento) => setCodigo(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
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
            {creando ? "Creando..." : "Crear permiso"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {permisos.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : permisos.error ? (
          <div className="p-4">
            <ErrorBanner error={permisos.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Código</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Descripción</th>
              </tr>
            </thead>
            <tbody>
              {permisos.datos.map((permiso) => (
                <tr key={permiso.id} className="border-b border-slate-100">
                  <td className="px-4 py-2 text-slate-900">{permiso.id}</td>
                  <td className="px-4 py-2 font-mono text-xs text-slate-900">{permiso.codigo}</td>
                  <td className="px-4 py-2 text-slate-900">{permiso.nombre}</td>
                  <td className="px-4 py-2 text-slate-900">{permiso.descripcion ?? "—"}</td>
                </tr>
              ))}
              {permisos.datos.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-3 text-sm text-slate-500">
                    No hay permisos registrados.
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
