import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorSedeGlobal } from "../SelectorSedeGlobal";
import { ApiRequestError } from "../../services/apiClient";
import { entregasService } from "../../services/entregasService";
import type { DetalleEntrega, Entrega } from "../../types/entrega";

interface EntregaDetalleRowProps {
  entregaId: number;
  detalle: DetalleEntrega;
  nombreProducto: string;
  editable: boolean;
  onActualizado: (entrega: Entrega) => void;
}

// Fila de un DetalleEntrega: cantidad entregada de ese producto, distribuciones por sede
// (RN-035/ADR-019: cada distribución conserva una fotografía histórica de la Sede al momento de
// registrarse, no los datos actuales) y un formulario para agregar una distribución nueva.
export function EntregaDetalleRow({ entregaId, detalle, nombreProducto, editable, onActualizado }: EntregaDetalleRowProps) {
  const [sedeId, setSedeId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  const cantidadDistribuida = detalle.distribuciones.reduce((suma, distribucion) => suma + distribucion.cantidad, 0);
  const distribucionCompleta = cantidadDistribuida === detalle.cantidadEntregada;

  async function agregarDistribucion(evento: React.FormEvent) {
    evento.preventDefault();
    if (sedeId == null) return;

    setGuardando(true);
    setError(null);
    try {
      const resultado = await entregasService.agregarDistribucion(entregaId, detalle.id, {
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
        <td className="py-2 pr-3 text-slate-900">{detalle.cantidadEntregada}</td>
        <td className="py-2 text-slate-900">
          {cantidadDistribuida} {distribucionCompleta ? "(completa)" : `de ${detalle.cantidadEntregada}`}
        </td>
      </tr>
      <tr className="border-b border-slate-100 last:border-0">
        <td colSpan={3} className="pb-3">
          {detalle.distribuciones.length > 0 && (
            <ul className="mb-2 flex flex-wrap gap-2 text-xs text-slate-600">
              {detalle.distribuciones.map((distribucion) => (
                <li key={distribucion.id} className="rounded bg-slate-100 px-2 py-1">
                  Sede #{distribucion.sedeId}: {distribucion.cantidad}
                  {distribucion.ciudadEntrega ? ` — ${distribucion.ciudadEntrega}` : ""}
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
