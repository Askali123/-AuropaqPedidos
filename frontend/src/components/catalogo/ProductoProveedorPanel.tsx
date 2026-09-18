import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorCatalogo } from "../requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Producto, ProductoProveedor, Proveedor } from "../../types/catalogos";

interface ProductoProveedorPanelProps {
  producto: Producto;
  proveedores: Proveedor[];
}

// docs/05-api.md §30 (TASK-019). Panel expandible por fila de ProductosSeccion (mismo patrón que
// SedesPanel bajo EmpresasSeccion): permite asociar un Proveedor a este Producto con su propio
// código/descripción/categoría/unidad (CLAUDE.md §23 — un producto interno puede tener
// identificadores distintos según el proveedor) y editar esa asociación en línea. Sin DELETE: no
// existe en el backend.
export function ProductoProveedorPanel({ producto, proveedores }: ProductoProveedorPanelProps) {
  const [version, setVersion] = useState(0);
  const asociaciones = useConsultaLista<ProductoProveedor>(
    () => catalogosService.listarProveedoresDeProducto(producto.id),
    [producto.id, version],
  );

  const [formAbierto, setFormAbierto] = useState(false);
  const [proveedorId, setProveedorId] = useState<number | null>(null);
  const [codigoProveedor, setCodigoProveedor] = useState("");
  const [descripcionProveedor, setDescripcionProveedor] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);

  async function crearAsociacion(evento: React.FormEvent) {
    evento.preventDefault();
    if (proveedorId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.asociarProveedorAProducto(producto.id, {
        proveedorId,
        codigoProveedor,
        descripcionProveedor: descripcionProveedor || undefined,
      });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setProveedorId(null);
      setCodigoProveedor("");
      setDescripcionProveedor("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <div className="flex items-center justify-between">
        <h4 className="text-xs font-semibold text-slate-600">Proveedores de este producto</h4>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
        >
          {formAbierto ? "Cancelar" : "Asociar proveedor"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearAsociacion} className="mt-2 grid gap-2 sm:grid-cols-4">
          <SelectorCatalogo
            etiqueta="Proveedor"
            datos={proveedores}
            cargando={false}
            error={null}
            valorSeleccionado={proveedorId}
            onSeleccionar={setProveedorId}
            obtenerId={(proveedor) => proveedor.id}
            obtenerTexto={(proveedor) => proveedor.nombre}
            mensajeVacio="No hay proveedores registrados."
          />
          <input
            required
            placeholder="Código del proveedor"
            value={codigoProveedor}
            onChange={(evento) => setCodigoProveedor(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
          <input
            placeholder="Descripción del proveedor (opcional)"
            value={descripcionProveedor}
            onChange={(evento) => setDescripcionProveedor(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
          <button
            type="submit"
            disabled={proveedorId == null || creando}
            className="rounded bg-slate-700 px-3 py-1 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Asociando..." : "Asociar"}
          </button>
        </form>
      )}

      {errorCrear && (
        <div className="mt-2">
          <ErrorBanner error={errorCrear} />
        </div>
      )}

      <div className="mt-3">
        {asociaciones.cargando ? (
          <p className="text-sm text-slate-500">Cargando...</p>
        ) : asociaciones.error ? (
          <ErrorBanner error={asociaciones.error} />
        ) : asociaciones.datos.length === 0 ? (
          <p className="text-sm text-slate-500">Este producto todavía no tiene proveedores asociados.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="py-1 pr-3 font-medium">Proveedor</th>
                <th className="py-1 pr-3 font-medium">Código</th>
                <th className="py-1 pr-3 font-medium">Descripción</th>
                <th className="py-1 pr-3 font-medium">Activo</th>
                <th className="py-1 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {asociaciones.datos.map((asociacion) =>
                editandoId === asociacion.id ? (
                  <FilaEdicion
                    key={asociacion.id}
                    productoId={producto.id}
                    asociacion={asociacion}
                    onCancelar={() => setEditandoId(null)}
                    onGuardado={() => {
                      setEditandoId(null);
                      setVersion((v) => v + 1);
                    }}
                  />
                ) : (
                  <tr key={asociacion.id} className="border-b border-slate-100">
                    <td className="py-1 pr-3 text-slate-900">{asociacion.proveedorNombre}</td>
                    <td className="py-1 pr-3 text-slate-900">{asociacion.codigoProveedor}</td>
                    <td className="py-1 pr-3 text-slate-900">{asociacion.descripcionProveedor ?? "—"}</td>
                    <td className="py-1 pr-3 text-slate-900">{asociacion.activo ? "Sí" : "No"}</td>
                    <td className="py-1">
                      <button
                        type="button"
                        onClick={() => setEditandoId(asociacion.id)}
                        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                      >
                        Editar
                      </button>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

function FilaEdicion({
  productoId,
  asociacion,
  onCancelar,
  onGuardado,
}: {
  productoId: number;
  asociacion: ProductoProveedor;
  onCancelar: () => void;
  onGuardado: () => void;
}) {
  const [codigoProveedor, setCodigoProveedor] = useState(asociacion.codigoProveedor);
  const [descripcionProveedor, setDescripcionProveedor] = useState(asociacion.descripcionProveedor ?? "");
  const [activo, setActivo] = useState(asociacion.activo);
  // CategoriaProveedor/UnidadProveedor no tienen input en esta fila compacta — se reenvían tal
  // cual venían para no perderlos en el PUT (que reemplaza todo el objeto,
  // ActualizarProductoProveedorRequest).
  const categoriaProveedor = asociacion.categoriaProveedor ?? "";
  const unidadProveedor = asociacion.unidadProveedor ?? "";
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarProductoProveedor(productoId, asociacion.id, {
        codigoProveedor,
        descripcionProveedor: descripcionProveedor || null,
        categoriaProveedor: categoriaProveedor || null,
        unidadProveedor: unidadProveedor || null,
        activo,
      });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100">
      <td className="py-1 pr-3 text-slate-900">{asociacion.proveedorNombre}</td>
      <td className="py-1 pr-3">
        <input
          value={codigoProveedor}
          onChange={(evento) => setCodigoProveedor(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="py-1 pr-3">
        <input
          value={descripcionProveedor}
          onChange={(evento) => setDescripcionProveedor(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="py-1 pr-3">
        <input type="checkbox" checked={activo} onChange={(evento) => setActivo(evento.target.checked)} />
      </td>
      <td className="py-1">
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={guardar}
            disabled={guardando}
            className="rounded bg-blue-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
          >
            {guardando ? "..." : "Guardar"}
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
