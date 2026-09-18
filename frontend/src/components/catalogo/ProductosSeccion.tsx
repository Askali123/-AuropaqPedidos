import { Fragment, useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { ProductoProveedorPanel } from "./ProductoProveedorPanel";
import { SelectorCatalogo } from "../requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Categoria, Producto, Proveedor, UnidadMedida } from "../../types/catalogos";

// docs/05-api.md §13. CategoriaId/UnidadMedidaId son obligatorios (RN-021, invariante de Domain)
// tanto al crear como al editar — por eso este componente carga las listas de Categoría/
// UnidadMedida (CategoriasSeccion/UnidadesMedidaSeccion cubren su propia alta; aquí solo se
// seleccionan). Sin DELETE: no existe en el backend. Panel expandible por fila con sus
// asociaciones Producto-Proveedor (ProductoProveedorPanel, §30/TASK-019, CLAUDE.md §23) — mismo
// patrón que EmpresasSeccion/SedesPanel.
export function ProductosSeccion() {
  const [version, setVersion] = useState(0);
  const productos = useConsultaLista(() => catalogosService.listarProductos(), [version]);
  const categorias = useConsultaLista<Categoria>(() => catalogosService.listarCategorias(), []);
  const unidades = useConsultaLista<UnidadMedida>(() => catalogosService.listarUnidadesMedida(), []);
  const proveedores = useConsultaLista<Proveedor>(() => catalogosService.listarProveedores(), []);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [categoriaId, setCategoriaId] = useState<number | null>(null);
  const [unidadMedidaId, setUnidadMedidaId] = useState<number | null>(null);
  const [codigoInterno, setCodigoInterno] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [expandidoId, setExpandidoId] = useState<number | null>(null);

  async function crearProducto(evento: React.FormEvent) {
    evento.preventDefault();
    if (categoriaId == null || unidadMedidaId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.crearProducto({
        nombre,
        categoriaId,
        unidadMedidaId,
        codigoInterno: codigoInterno || undefined,
        descripcion: descripcion || undefined,
      });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setNombre("");
      setCategoriaId(null);
      setUnidadMedidaId(null);
      setCodigoInterno("");
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
        <h2 className="text-base font-semibold text-slate-800">Productos</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nuevo producto"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearProducto} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
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
              <label className="text-sm font-medium text-slate-700">Código interno (opcional)</label>
              <input
                value={codigoInterno}
                onChange={(evento) => setCodigoInterno(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <SelectorCatalogo
              etiqueta="Categoría"
              datos={categorias.datos}
              cargando={categorias.cargando}
              error={categorias.error}
              valorSeleccionado={categoriaId}
              onSeleccionar={setCategoriaId}
              obtenerId={(categoria) => categoria.id}
              obtenerTexto={(categoria) => categoria.nombre}
              mensajeVacio="No hay categorías registradas."
            />
            <SelectorCatalogo
              etiqueta="Unidad de medida"
              datos={unidades.datos}
              cargando={unidades.cargando}
              error={unidades.error}
              valorSeleccionado={unidadMedidaId}
              onSeleccionar={setUnidadMedidaId}
              obtenerId={(unidad) => unidad.id}
              obtenerTexto={(unidad) => `${unidad.codigo} — ${unidad.nombre}`}
              mensajeVacio="No hay unidades de medida registradas."
            />
            <div className="flex flex-col gap-1 sm:col-span-2">
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
            disabled={categoriaId == null || unidadMedidaId == null || creando}
            className="mt-3 rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Creando..." : "Crear producto"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {productos.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : productos.error ? (
          <div className="p-4">
            <ErrorBanner error={productos.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Código</th>
                <th className="px-4 py-2 font-medium">Categoría</th>
                <th className="px-4 py-2 font-medium">Unidad</th>
                <th className="px-4 py-2 font-medium">Activo</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {productos.datos.map((producto) => (
                <Fragment key={producto.id}>
                  {editandoId === producto.id ? (
                    <FilaEdicion
                      producto={producto}
                      categorias={categorias.datos}
                      unidades={unidades.datos}
                      onCancelar={() => setEditandoId(null)}
                      onGuardado={() => {
                        setEditandoId(null);
                        setVersion((v) => v + 1);
                      }}
                    />
                  ) : (
                    <tr className="border-b border-slate-100">
                      <td className="px-4 py-2 text-slate-900">{producto.id}</td>
                      <td className="px-4 py-2 text-slate-900">{producto.nombre}</td>
                      <td className="px-4 py-2 text-slate-900">{producto.codigoInterno ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{producto.categoriaNombre}</td>
                      <td className="px-4 py-2 text-slate-900">{producto.unidadMedidaCodigo}</td>
                      <td className="px-4 py-2 text-slate-900">{producto.activo ? "Sí" : "No"}</td>
                      <td className="px-4 py-2">
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            onClick={() => setEditandoId(producto.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            Editar
                          </button>
                          <button
                            type="button"
                            onClick={() => setExpandidoId(expandidoId === producto.id ? null : producto.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            {expandidoId === producto.id ? "Ocultar proveedores" : "Ver proveedores"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                  {expandidoId === producto.id && (
                    <tr>
                      <td colSpan={7} className="bg-slate-50 px-4 py-3">
                        <ProductoProveedorPanel producto={producto} proveedores={proveedores.datos} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {productos.datos.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-3 text-sm text-slate-500">
                    No hay productos registrados.
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
  producto,
  categorias,
  unidades,
  onCancelar,
  onGuardado,
}: {
  producto: Producto;
  categorias: Categoria[];
  unidades: UnidadMedida[];
  onCancelar: () => void;
  onGuardado: () => void;
}) {
  const [nombre, setNombre] = useState(producto.nombre);
  const [categoriaId, setCategoriaId] = useState(producto.categoriaId);
  const [unidadMedidaId, setUnidadMedidaId] = useState(producto.unidadMedidaId);
  const [codigoInterno, setCodigoInterno] = useState(producto.codigoInterno ?? "");
  const [activo, setActivo] = useState(producto.activo);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);
  // Descripción no tiene input en esta fila compacta — se reenvía tal cual venía para no
  // perderla en el PUT (que reemplaza todo el objeto, ActualizarProductoRequest).
  const descripcion = producto.descripcion ?? "";

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarProducto(producto.id, {
        nombre,
        categoriaId,
        unidadMedidaId,
        codigoInterno: codigoInterno || null,
        descripcion: descripcion || null,
        activo,
      });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100 bg-slate-50">
      <td className="px-4 py-2 text-slate-900">{producto.id}</td>
      <td className="px-4 py-2">
        <input
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          value={codigoInterno}
          onChange={(evento) => setCodigoInterno(evento.target.value)}
          className="w-24 rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <select
          value={categoriaId}
          onChange={(evento) => setCategoriaId(Number(evento.target.value))}
          className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        >
          {categorias.map((categoria) => (
            <option key={categoria.id} value={categoria.id}>
              {categoria.nombre}
            </option>
          ))}
        </select>
      </td>
      <td className="px-4 py-2">
        <select
          value={unidadMedidaId}
          onChange={(evento) => setUnidadMedidaId(Number(evento.target.value))}
          className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        >
          {unidades.map((unidad) => (
            <option key={unidad.id} value={unidad.id}>
              {unidad.codigo}
            </option>
          ))}
        </select>
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
