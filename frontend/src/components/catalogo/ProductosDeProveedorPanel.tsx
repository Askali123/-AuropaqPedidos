import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { catalogosService } from "../../services/catalogosService";
import type { ProductoProveedor } from "../../types/catalogos";

interface ProductosDeProveedorPanelProps {
  proveedorId: number;
}

// docs/05-api.md §30 (TASK-019), lado inverso de ProductoProveedorPanel. Solo lectura: la
// asociación siempre se crea desde la pantalla de Productos (un único punto de alta, sin
// duplicar el formulario aquí) — este panel consulta la misma relación desde el punto de vista
// del Proveedor.
export function ProductosDeProveedorPanel({ proveedorId }: ProductosDeProveedorPanelProps) {
  const asociaciones = useConsultaLista<ProductoProveedor>(
    () => catalogosService.listarProductosDeProveedor(proveedorId),
    [proveedorId],
  );

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <h4 className="text-xs font-semibold text-slate-600">Productos de este proveedor</h4>

      <div className="mt-3">
        {asociaciones.cargando ? (
          <p className="text-sm text-slate-500">Cargando...</p>
        ) : asociaciones.error ? (
          <ErrorBanner error={asociaciones.error} />
        ) : asociaciones.datos.length === 0 ? (
          <p className="text-sm text-slate-500">
            Este proveedor todavía no tiene productos asociados (se asocian desde "Productos").
          </p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="py-1 pr-3 font-medium">Producto</th>
                <th className="py-1 pr-3 font-medium">Código del proveedor</th>
                <th className="py-1 pr-3 font-medium">Descripción</th>
                <th className="py-1 font-medium">Activo</th>
              </tr>
            </thead>
            <tbody>
              {asociaciones.datos.map((asociacion) => (
                <tr key={asociacion.id} className="border-b border-slate-100">
                  <td className="py-1 pr-3 text-slate-900">{asociacion.productoNombre}</td>
                  <td className="py-1 pr-3 text-slate-900">{asociacion.codigoProveedor}</td>
                  <td className="py-1 pr-3 text-slate-900">{asociacion.descripcionProveedor ?? "—"}</td>
                  <td className="py-1 text-slate-900">{asociacion.activo ? "Sí" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
