import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorCatalogo } from "./SelectorCatalogo";
import { ApiRequestError } from "../../services/apiClient";
import { requisicionesService } from "../../services/requisicionesService";
import type { Producto } from "../../types/catalogos";
import type { Requisicion } from "../../types/requisicion";

interface AgregarDetalleFormProps {
  requisicionId: number;
  productos: Producto[];
  productosCargando: boolean;
  productosError: ApiRequestError | null;
  onAgregado: (requisicion: Requisicion) => void;
}

// Formulario de una sola responsabilidad: agregar UN producto a la requisición. La validación de
// "cantidad > 0" aquí es solo UX (atributo min + disabled del botón); la regla real la aplica el
// backend (docs/05-api.md §54.4, fila 2) y se muestra vía ErrorBanner si falla.
export function AgregarDetalleForm({
  requisicionId,
  productos,
  productosCargando,
  productosError,
  onAgregado,
}: AgregarDetalleFormProps) {
  const [productoId, setProductoId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState("");
  const [observacion, setObservacion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function agregar(evento: React.FormEvent) {
    evento.preventDefault();
    if (productoId == null) return;

    setEnviando(true);
    setError(null);
    try {
      const resultado = await requisicionesService.agregarDetalle(requisicionId, {
        productoId,
        cantidadSolicitada: Number(cantidad),
        observacion: observacion || undefined,
      });
      onAgregado(resultado);
      setProductoId(null);
      setCantidad("");
      setObservacion("");
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={agregar} className="flex flex-col gap-3 sm:flex-row sm:items-end sm:gap-4">
      <div className="flex-1">
        <SelectorCatalogo
          etiqueta="Producto"
          datos={productos}
          cargando={productosCargando}
          error={productosError}
          valorSeleccionado={productoId}
          onSeleccionar={setProductoId}
          obtenerId={(producto) => producto.id}
          obtenerTexto={(producto) =>
            `${producto.nombre} (${producto.unidadMedidaCodigo})` + (producto.activo ? "" : " — inactivo")
          }
          mensajeVacio="No hay productos en el catálogo."
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

      <div className="flex flex-1 flex-col gap-1">
        <label className="text-sm font-medium text-slate-700">Observación (opcional)</label>
        <input
          type="text"
          value={observacion}
          onChange={(evento) => setObservacion(evento.target.value)}
          className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
        />
      </div>

      <button
        type="submit"
        disabled={enviando || productoId == null || !cantidad}
        className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {enviando ? "Agregando..." : "Agregar producto"}
      </button>

      {error && (
        <div className="w-full sm:basis-full">
          <ErrorBanner error={error} />
        </div>
      )}
    </form>
  );
}
