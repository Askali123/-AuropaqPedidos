import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { ApiRequestError } from "../../services/apiClient";
import { requisicionesService } from "../../services/requisicionesService";
import type { DistribucionRequisicion, Requisicion } from "../../types/requisicion";

interface DistribucionRequisicionRowProps {
  requisicionId: number;
  detalleId: number;
  distribucion: DistribucionRequisicion;
  nombreSede: string;
  // detalle.cantidadSolicitada - (detalle.cantidadDistribuida - distribucion.cantidad): el mismo
  // límite que valida DetalleRequisicion.ModificarDistribucion (Domain) para ESTA distribución en
  // particular. Solo es una ayuda de UX (deshabilita el botón); el backend sigue validando.
  maximoDisponibleParaModificar: number;
  editable: boolean;
  onActualizado: (requisicion: Requisicion) => void;
}

// Una fila = una distribución: editar su cantidad o eliminarla. Mismo patrón que
// DetalleRequisicionRow (estado local de edición/carga/error, sube la Requisicion completa que
// devuelve la Api al padre en cada éxito).
export function DistribucionRequisicionRow({
  requisicionId,
  detalleId,
  distribucion,
  nombreSede,
  maximoDisponibleParaModificar,
  editable,
  onActualizado,
}: DistribucionRequisicionRowProps) {
  const [enEdicion, setEnEdicion] = useState(false);
  const [nuevaCantidad, setNuevaCantidad] = useState(String(distribucion.cantidad));
  const [guardando, setGuardando] = useState(false);
  const [eliminando, setEliminando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  function iniciarEdicion() {
    setNuevaCantidad(String(distribucion.cantidad));
    setError(null);
    setEnEdicion(true);
  }

  async function guardarCantidad() {
    setGuardando(true);
    setError(null);
    try {
      const resultado = await requisicionesService.modificarDistribucion(requisicionId, detalleId, distribucion.id, {
        nuevaCantidad: Number(nuevaCantidad),
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
      const resultado = await requisicionesService.eliminarDistribucion(requisicionId, detalleId, distribucion.id);
      onActualizado(resultado);
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setEliminando(false);
    }
  }

  const cantidadInvalida = enEdicion && (!nuevaCantidad || Number(nuevaCantidad) <= 0);
  const excedeMaximo = enEdicion && Number(nuevaCantidad) > maximoDisponibleParaModificar;

  return (
    <li className="border-b border-slate-200 py-1.5 last:border-0">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <span className="text-slate-700">{nombreSede}</span>

        {enEdicion ? (
          <div className="flex items-center gap-2">
            <input
              type="number"
              min={1}
              value={nuevaCantidad}
              onChange={(evento) => setNuevaCantidad(evento.target.value)}
              className="w-20 rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
            />
            <button
              type="button"
              onClick={guardarCantidad}
              disabled={guardando || cantidadInvalida || excedeMaximo}
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
          <div className="flex items-center gap-2">
            <span className="font-medium text-slate-900">{distribucion.cantidad}</span>
            {editable && (
              <>
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
              </>
            )}
          </div>
        )}
      </div>

      {excedeMaximo && (
        <p className="mt-1 text-xs text-amber-600">Máximo disponible para esta distribución: {maximoDisponibleParaModificar}.</p>
      )}
      {error && (
        <div className="mt-2">
          <ErrorBanner error={error} />
        </div>
      )}
    </li>
  );
}
