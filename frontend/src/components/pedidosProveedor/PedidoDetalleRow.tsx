import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorSedeGlobal } from "../SelectorSedeGlobal";
import { ApiRequestError } from "../../services/apiClient";
import { pedidosProveedorService } from "../../services/pedidosProveedorService";
import type { DetallePedidoProveedor, PedidoProveedor } from "../../types/pedidoProveedor";

interface PedidoDetalleRowProps {
  pedidoId: number;
  detalle: DetallePedidoProveedor;
  nombreProducto: string;
  editable: boolean;
  onActualizado: (pedido: PedidoProveedor) => void;
}

// Fila de un DetallePedidoProveedor: cantidad necesaria/pedida, distribuciones ya registradas, y
// un formulario para agregar una distribución nueva (sede de cualquier empresa —
// SelectorSedeGlobal, CLAUDE.md §27). Sin editar/eliminar distribuciones: el backend no expone
// PUT/DELETE para ellas (mismo criterio de minimalismo que el resto de Pedido/Entrega/Factura).
export function PedidoDetalleRow({ pedidoId, detalle, nombreProducto, editable, onActualizado }: PedidoDetalleRowProps) {
  const [sedeId, setSedeId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  const cantidadDistribuida = detalle.distribuciones.reduce((suma, distribucion) => suma + distribucion.cantidad, 0);
  const distribucionCompleta = cantidadDistribuida === detalle.cantidadPedida;

  async function agregarDistribucion(evento: React.FormEvent) {
    evento.preventDefault();
    if (sedeId == null) return;

    setGuardando(true);
    setError(null);
    try {
      const resultado = await pedidosProveedorService.agregarDistribucion(pedidoId, detalle.id, {
        sedeId,
        cantidad: Number(cantidad),
      });
      onActualizado(resultado);
      setSedeId(null);
      setCantidad("");
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <>
      <tr className="border-b border-slate-100">
        <td className="py-2 pr-3 text-slate-900">{nombreProducto}</td>
        <td className="py-2 pr-3 text-slate-900">{detalle.cantidadNecesaria}</td>
        <td className="py-2 pr-3 text-slate-900">{detalle.cantidadPedida}</td>
        <td className="py-2 pr-3 text-slate-900">{detalle.precioUnitario ?? "—"}</td>
        <td className="py-2 text-slate-900">
          {cantidadDistribuida} {distribucionCompleta ? "(completa)" : `de ${detalle.cantidadPedida}`}
        </td>
      </tr>
      <tr className="border-b border-slate-100 last:border-0">
        <td colSpan={5} className="pb-3">
          {detalle.distribuciones.length > 0 && (
            <ul className="mb-2 flex flex-wrap gap-2 text-xs text-slate-600">
              {detalle.distribuciones.map((distribucion) => (
                <li key={distribucion.id} className="rounded bg-slate-100 px-2 py-1">
                  Sede #{distribucion.sedeId}: {distribucion.cantidad}
                </li>
              ))}
            </ul>
          )}

          {editable && !distribucionCompleta && (
            <form onSubmit={agregarDistribucion} className="flex flex-wrap items-end gap-2">
              <div className="min-w-[20rem]">
                <SelectorSedeGlobal valorSeleccionado={sedeId} onSeleccionar={setSedeId} deshabilitado={guardando} />
              </div>
              <div className="flex flex-col gap-1">
                <label className="text-sm font-medium text-slate-700">Cantidad</label>
                <input
                  type="number"
                  min={1}
                  value={cantidad}
                  onChange={(evento) => setCantidad(evento.target.value)}
                  required
                  disabled={guardando}
                  className="w-24 rounded border border-slate-300 bg-white px-2 py-2 text-sm text-slate-900 disabled:opacity-50"
                />
              </div>
              <button
                type="submit"
                disabled={guardando || sedeId == null}
                className="rounded bg-slate-700 px-3 py-2 text-xs font-medium text-white disabled:opacity-50"
              >
                {guardando ? "Agregando..." : "Agregar distribución"}
              </button>
            </form>
          )}

          {error && (
            <div className="mt-2">
              <ErrorBanner error={error} />
            </div>
          )}
        </td>
      </tr>
    </>
  );
}
