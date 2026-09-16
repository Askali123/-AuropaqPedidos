import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Categoria } from "../../types/catalogos";

// docs/05-api.md §14. Lista + alta + edición en línea (Nombre/Descripcion/Activo viajan juntos
// en el mismo PUT, §14 — mismo criterio que Empresa/Sede/UnidadMedida/Producto/Proveedor). Sin
// DELETE: no existe en el backend.
export function CategoriasSeccion() {
  const [version, setVersion] = useState(0);
  const categorias = useConsultaLista(() => catalogosService.listarCategorias(), [version]);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);

  async function crearCategoria(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.crearCategoria({ nombre, descripcion: descripcion || undefined });
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
        <h2 className="text-base font-semibold text-slate-800">Categorías</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nueva categoría"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearCategoria} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
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
            {creando ? "Creando..." : "Crear categoría"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {categorias.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : categorias.error ? (
          <div className="p-4">
            <ErrorBanner error={categorias.error} />
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
              {categorias.datos.map((categoria) =>
                editandoId === categoria.id ? (
                  <FilaEdicion
                    key={categoria.id}
                    categoria={categoria}
                    onCancelar={() => setEditandoId(null)}
                    onGuardado={() => {
                      setEditandoId(null);
                      setVersion((v) => v + 1);
                    }}
                  />
                ) : (
                  <tr key={categoria.id} className="border-b border-slate-100">
                    <td className="px-4 py-2 text-slate-900">{categoria.id}</td>
                    <td className="px-4 py-2 text-slate-900">{categoria.nombre}</td>
                    <td className="px-4 py-2 text-slate-900">{categoria.descripcion ?? "—"}</td>
                    <td className="px-4 py-2 text-slate-900">{categoria.activo ? "Sí" : "No"}</td>
                    <td className="px-4 py-2">
                      <button
                        type="button"
                        onClick={() => setEditandoId(categoria.id)}
                        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                      >
                        Editar
                      </button>
                    </td>
                  </tr>
                ),
              )}
              {categorias.datos.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-3 text-sm text-slate-500">
                    No hay categorías registradas.
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
  categoria,
  onCancelar,
  onGuardado,
}: {
  categoria: Categoria;
  onCancelar: () => void;
  onGuardado: () => void;
}) {
  const [nombre, setNombre] = useState(categoria.nombre);
  const [descripcion, setDescripcion] = useState(categoria.descripcion ?? "");
  const [activo, setActivo] = useState(categoria.activo);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarCategoria(categoria.id, { nombre, descripcion: descripcion || null, activo });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100 bg-slate-50">
      <td className="px-4 py-2 text-slate-900">{categoria.id}</td>
      <td className="px-4 py-2">
        <input
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          value={descripcion}
          onChange={(evento) => setDescripcion(evento.target.value)}
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
        {error && (
          <div className="mt-1">
            <ErrorBanner error={error} />
          </div>
        )}
      </td>
    </tr>
  );
}
