import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { ApiRequestError } from "../../services/apiClient";
import { solicitudesProductoService } from "../../services/solicitudesProductoService";

interface SolicitarProductoFormProps {
  onSolicitado: () => void;
}

// docs/05-api.md §28.1. Exige PRODUCTO_SOLICITAR (rol Solicitante) — distinto de PRODUCTO_CREAR
// (gestor de catálogo, resuelve la solicitud en ResolverSolicitudPanel). Mismo patrón de
// formulario colapsable que CrearPeriodoForm.
export function SolicitarProductoForm({ onSolicitado }: SolicitarProductoFormProps) {
  const [abierto, setAbierto] = useState(false);
  const [nombreSolicitado, setNombreSolicitado] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [observacion, setObservacion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function solicitar(evento: React.FormEvent) {
    evento.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      await solicitudesProductoService.solicitar({
        nombreSolicitado,
        descripcion: descripcion || undefined,
        observacion: observacion || undefined,
      });
      onSolicitado();
      setAbierto(false);
      setNombreSolicitado("");
      setDescripcion("");
      setObservacion("");
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setEnviando(false);
    }
  }

  if (!abierto) {
    return (
      <button
        type="button"
        onClick={() => setAbierto(true)}
        className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
      >
        Solicitar producto
      </button>
    );
  }

  return (
    <form onSubmit={solicitar} className="rounded border border-slate-200 bg-slate-50 p-3">
      <div className="grid gap-3 sm:grid-cols-2">
        <div className="flex flex-col gap-1 sm:col-span-2">
          <label className="text-sm font-medium text-slate-700">Nombre del producto</label>
          <input
            required
            value={nombreSolicitado}
            onChange={(evento) => setNombreSolicitado(evento.target.value)}
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
        <div className="flex flex-col gap-1">
          <label className="text-sm font-medium text-slate-700">Observación (opcional)</label>
          <input
            value={observacion}
            onChange={(evento) => setObservacion(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
          />
        </div>
      </div>

      <div className="mt-3 flex items-center gap-3">
        <button
          type="submit"
          disabled={enviando}
          className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {enviando ? "Enviando..." : "Enviar solicitud"}
        </button>
        <button type="button" onClick={() => setAbierto(false)} disabled={enviando} className="text-sm font-medium text-slate-500">
          Cancelar
        </button>
      </div>

      {error && (
        <div className="mt-2">
          <ErrorBanner error={error} />
        </div>
      )}
    </form>
  );
}
