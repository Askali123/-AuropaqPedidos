import { Fragment, useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SedesPanel } from "./SedesPanel";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Empresa } from "../../types/catalogos";

// docs/05-api.md §11. Lista + alta + edición en línea + panel expandible por fila con sus Sedes
// (SedesPanel, §12). Sin DELETE: no existe en el backend.
export function EmpresasSeccion() {
  const [version, setVersion] = useState(0);
  const empresas = useConsultaLista(() => catalogosService.listarEmpresas(), [version]);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [nit, setNit] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [expandidoId, setExpandidoId] = useState<number | null>(null);

  async function crearEmpresa(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.crearEmpresa({ nombre, nit: nit || undefined });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setNombre("");
      setNit("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-slate-800">Empresas</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nueva empresa"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearEmpresa} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
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
              <label className="text-sm font-medium text-slate-700">Nit (opcional)</label>
              <input
                value={nit}
                onChange={(evento) => setNit(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={creando}
            className="mt-3 rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Creando..." : "Crear empresa"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {empresas.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : empresas.error ? (
          <div className="p-4">
            <ErrorBanner error={empresas.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Activo</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {empresas.datos.map((empresa) => (
                <Fragment key={empresa.id}>
                  {editandoId === empresa.id ? (
                    <FilaEdicion
                      empresa={empresa}
                      onCancelar={() => setEditandoId(null)}
                      onGuardado={() => {
                        setEditandoId(null);
                        setVersion((v) => v + 1);
                      }}
                    />
                  ) : (
                    <tr className="border-b border-slate-100">
                      <td className="px-4 py-2 text-slate-900">{empresa.id}</td>
                      <td className="px-4 py-2 text-slate-900">{empresa.nombre}</td>
                      <td className="px-4 py-2 text-slate-900">{empresa.activo ? "Sí" : "No"}</td>
                      <td className="px-4 py-2">
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            onClick={() => setEditandoId(empresa.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            Editar
                          </button>
                          <button
                            type="button"
                            onClick={() => setExpandidoId(expandidoId === empresa.id ? null : empresa.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            {expandidoId === empresa.id ? "Ocultar sedes" : "Ver sedes"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                  {expandidoId === empresa.id && (
                    <tr>
                      <td colSpan={4} className="bg-slate-50 px-4 py-3">
                        <SedesPanel empresaId={empresa.id} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {empresas.datos.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-3 text-sm text-slate-500">
                    No hay empresas registradas.
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

function FilaEdicion({
  empresa,
  onCancelar,
  onGuardado,
}: {
  empresa: Empresa;
  onCancelar: () => void;
  onGuardado: () => void;
}) {
  const [nombre, setNombre] = useState(empresa.nombre);
  const [nit, setNit] = useState("");
  const [activo, setActivo] = useState(empresa.activo);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarEmpresa(empresa.id, { nombre, nit: nit || null, activo });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100 bg-slate-50">
      <td className="px-4 py-2 text-slate-900">{empresa.id}</td>
      <td className="px-4 py-2">
        <input
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input type="checkbox" checked={activo} onChange={(evento) => setActivo(evento.target.checked)} />
      </td>
      <td className="px-4 py-2">
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={guardar}
            disabled={guardando}
            className="rounded bg-blue-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
          >
            {guardando ? "Guardando..." : "Guardar"}
          </button>
          <button type="button" onClick={onCancelar} disabled={guardando} className="text-xs font-medium text-slate-500">
            Cancelar
          </button>
        </div>
        <div className="mt-1 flex items-center gap-1">
          <input
            placeholder="Nit (dejar vacío borra el actual)"
            value={nit}
            onChange={(evento) => setNit(evento.target.value)}
            className="w-full rounded border border-amber-300 bg-white px-2 py-1 text-xs text-slate-900"
          />
        </div>
        <p className="mt-1 text-[11px] text-amber-700">
          GET /empresas no devuelve el Nit actual (docs/05-api.md §11.1) — este PUT reemplaza el
          valor completo, así que déjalo en blanco solo si quieres borrarlo.
        </p>
        {error && (
          <div className="mt-1">
            <ErrorBanner error={error} />
          </div>
        )}
      </td>
    </tr>
  );
}
