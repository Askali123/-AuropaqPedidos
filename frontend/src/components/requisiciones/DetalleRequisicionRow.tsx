import { useState } from "react";
import { DistribucionesDetalle } from "./DistribucionesDetalle";
import { ErrorBanner } from "../ErrorBanner";
import { ApiRequestError } from "../../services/apiClient";
import { requisicionesService } from "../../services/requisicionesService";
import type { Sede } from "../../types/catalogos";
import type { DetalleRequisicion, Requisicion } from "../../types/requisicion";

interface DetalleRequisicionRowProps {
  requisicionId: number;
  detalle: DetalleRequisicion;
  nombreProducto: string;
  editable: boolean;
  sedes: Sede[];
  sedesCargando: boolean;
  sedesError: ApiRequestError | null;
  onActualizado: (requisicion: Requisicion) => void;
}

// Dos filas de tabla por detalle: la primera edita/elimina el detalle (cantidad solicitada,
// observación); la segunda (colSpan completo) delega en DistribucionesDetalle la gestión de sus
// distribuciones por sede (tercera etapa, docs/05-api.md §54.4, filas 5-7). El estado de
// edición/carga/error del detalle es local a este componente; al tener éxito, sube la
// Requisicion completa que devolvió la Api (fuente de verdad) al padre.
export function DetalleRequisicionRow({
  requisicionId,
  detalle,
  nombreProducto,
  editable,
  sedes,
  sedesCargando,
  sedesError,
  onActualizado,
}: DetalleRequisicionRowProps) {
  const [enEdicion, setEnEdicion] = useState(false);
  const [nuevaCantidad, setNuevaCantidad] = useState(String(detalle.cantidadSolicitada));
  const [guardando, setGuardando] = useState(false);
  const [eliminando, setEliminando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  function iniciarEdicion() {
    setNuevaCantidad(String(detalle.cantidadSolicitada));
    setError(null);
    setEnEdicion(true);
  }

  async function guardarCantidad() {
    setGuardando(true);
    setError(null);
    try {
      const resultado = await requisicionesService.actualizarDetalle(requisicionId, detalle.id, {
        cantidadSolicitada: Number(nuevaCantidad),
      });
      onActualizado(resultado);
      setEnEdicion(false);
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setGuardando(false);
    }
  }

  async function eliminar() {
    setEliminando(true);
    setError(null);
    try {
      const resultado = await requisicionesService.eliminarDetalle(requisicionId, detalle.id);
      onActualizado(resultado);
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setEliminando(false);
    }
  }

  return (
    <>
      <tr className="border-b border-slate-100">
        <td className="py-2 pr-3 text-slate-900">{nombreProducto}</td>
        <td className="py-2 pr-3 text-slate-900">
          {enEdicion ? (
            <input
              type="number"
              min={1}
              value={nuevaCantidad}
              onChange={(evento) => setNuevaCantidad(evento.target.value)}
              className="w-20 rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
            />
          ) : (
            detalle.cantidadSolicitada
          )}
        </td>
        <td className="py-2 pr-3 text-slate-500">{detalle.observacion ?? "—"}</td>
        <td className="py-2 pr-3 text-slate-900">
          {detalle.cantidadDistribuida} {detalle.distribucionCompleta ? "(completa)" : ""}
        </td>
        <td className="py-2">
          {!editable ? (
            <span className="text-xs text-slate-400 italic">No editable en este estado</span>
          ) : enEdicion ? (
            <div className="flex gap-2">
              <button
                type="button"
                onClick={guardarCantidad}
                disabled={guardando}
                className="rounded bg-blue-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
              >
                {guardando ? "Guardando..." : "Guardar"}
              </button>
              <button
                type="button"
                onClick={() => setEnEdicion(false)}
                disabled={guardando}
                className="rounded border border-slate-300 px-2 py-1 text-xs font-medium text-slate-600"
              >
                Cancelar
              </button>
            </div>
          ) : (
            <div className="flex gap-2">
              <button
                type="button"
                onClick={iniciarEdicion}
                className="rounded border border-slate-300 px-2 py-1 text-xs font-medium text-slate-600 hover:bg-slate-50"
              >
                Editar
              </button>
              <button
                type="button"
                onClick={eliminar}
                disabled={eliminando}
                className="rounded border border-red-300 px-2 py-1 text-xs font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
              >
                {eliminando ? "Eliminando..." : "Eliminar"}
              </button>
            </div>
          )}
          {error && (
            <div className="mt-2">
              <ErrorBanner error={error} />
            </div>
          )}
        </td>
      </tr>
      <tr className="border-b border-slate-100 last:border-0">
        <td colSpan={5} className="pb-3">
          <DistribucionesDetalle
            requisicionId={requisicionId}
            detalle={detalle}
            sedes={sedes}
            sedesCargando={sedesCargando}
            sedesError={sedesError}
            editable={editable}
            onActualizado={onActualizado}
          />
        </td>
      </tr>
    </>
  );
}
