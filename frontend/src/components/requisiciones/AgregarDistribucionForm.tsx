import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorCatalogo } from "./SelectorCatalogo";
import { ApiRequestError } from "../../services/apiClient";
import { requisicionesService } from "../../services/requisicionesService";
import type { Sede } from "../../types/catalogos";
import type { Requisicion } from "../../types/requisicion";

interface AgregarDistribucionFormProps {
  requisicionId: number;
  detalleId: number;
  sedes: Sede[];
  sedesCargando: boolean;
  sedesError: ApiRequestError | null;
  // detalle.cantidadSolicitada - detalle.cantidadDistribuida (RN-011). Solo UX: deshabilita el
  // envío si es obviamente inválido; la suma real la vuelve a validar el backend.
  cantidadDisponible: number;
  onAgregado: (requisicion: Requisicion) => void;
}

// Formulario de una sola responsabilidad: agregar UNA distribución (sede + cantidad) a UN
// detalle. Reutiliza el mismo SelectorCatalogo y el mismo catálogo de Sede que ya consulta la
// página (docs/05-api.md §54.6.1) — no se vuelve a pedir aquí. Mismo criterio que
// AgregarDetalleForm: la validación de cantidad es solo UX, la real vive en el backend
// (docs/05-api.md §54.4, fila 5).
export function AgregarDistribucionForm({
  requisicionId,
  detalleId,
  sedes,
  sedesCargando,
  sedesError,
  cantidadDisponible,
  onAgregado,
}: AgregarDistribucionFormProps) {
  const [sedeId, setSedeId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function agregar(evento: React.FormEvent) {
    evento.preventDefault();
    if (sedeId == null) return;

    setEnviando(true);
    setError(null);
    try {
      const resultado = await requisicionesService.agregarDistribucion(requisicionId, detalleId, {
        sedeId,
        cantidad: Number(cantidad),
      });
      onAgregado(resultado);
      setSedeId(null);
      setCantidad("");
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setEnviando(false);
    }
  }

  if (cantidadDisponible <= 0) {
    return <p className="text-xs text-slate-400 italic">Ya se distribuyó toda la cantidad solicitada de este producto.</p>;
  }

  const excedeDisponible = cantidad !== "" && Number(cantidad) > cantidadDisponible;

  return (
    <form onSubmit={agregar} className="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-end sm:gap-3">
      <div className="w-full sm:w-48">
        <SelectorCatalogo
          etiqueta="Sede"
          datos={sedes}
          cargando={sedesCargando}
          error={sedesError}
          valorSeleccionado={sedeId}
          onSeleccionar={setSedeId}
          obtenerId={(sede) => sede.id}
          obtenerTexto={(sede) => sede.nombre + (sede.activo ? "" : " (inactiva)")}
          mensajeVacio="Esta empresa no tiene sedes registradas."
        />
      </div>

      <div className="flex flex-col gap-1">
        <label className="text-sm font-medium text-slate-700">Cantidad</label>
        <input
          type="number"
          min={1}
          required
          value={cantidad}
          onChange={(evento) => setCantidad(evento.target.value)}
          className="w-24 rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
        />
      </div>

      <button
        type="submit"
        disabled={enviando || sedeId == null || !cantidad || Number(cantidad) <= 0 || excedeDisponible}
        className="rounded bg-blue-600 px-3 py-2 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {enviando ? "Agregando..." : "Agregar distribución"}
      </button>

      {excedeDisponible && (
        <p className="w-full text-xs text-amber-600">Máximo disponible: {cantidadDisponible}.</p>
      )}
      {error && (
        <div className="w-full">
          <ErrorBanner error={error} />
        </div>
      )}
    </form>
  );
}
