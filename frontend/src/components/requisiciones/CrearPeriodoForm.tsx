import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Periodo } from "../../types/catalogos";

interface CrearPeriodoFormProps {
  onCreado: (periodo: Periodo) => void;
}

// docs/05-api.md §16.3. Formulario mínimo para crear un Periodo real (año/mes/fechas) sin
// depender de datos precargados — necesario para poder probar en vivo el flujo completo de
// estados de Requisición (Enviar/Iniciar revisión/Aprobar/Devolver), que exige una ventana de
// solicitud vigente. "Estado" no se pide aquí: el backend lo fija en "ABIERTO" al crear
// (CrearPeriodoUseCase, decisión explícita del usuario — RN-005 exige el campo pero ningún
// documento define sus valores válidos). La validación de "Año+Mes único" es responsabilidad
// exclusiva del backend; aquí solo se muestra el error si lo rechaza.
export function CrearPeriodoForm({ onCreado }: CrearPeriodoFormProps) {
  const [abierto, setAbierto] = useState(false);
  const [anio, setAnio] = useState(String(new Date().getFullYear()));
  const [mes, setMes] = useState(String(new Date().getMonth() + 1));
  const [fechaInicio, setFechaInicio] = useState("");
  const [fechaFin, setFechaFin] = useState("");
  const [fechaInicioSolicitud, setFechaInicioSolicitud] = useState("");
  const [fechaFinSolicitud, setFechaFinSolicitud] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function crear(evento: React.FormEvent) {
    evento.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      const periodo = await catalogosService.crearPeriodo({
        anio: Number(anio),
        mes: Number(mes),
        fechaInicio,
        fechaFin,
        fechaInicioSolicitud,
        fechaFinSolicitud,
      });
      onCreado(periodo);
      setAbierto(false);
      setFechaInicio("");
      setFechaFin("");
      setFechaInicioSolicitud("");
      setFechaFinSolicitud("");
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
        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
      >
        Crear un periodo nuevo
      </button>
    );
  }

  return (
    <form onSubmit={crear} className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Año</label>
          <input
            type="number"
            min={1}
            required
            value={anio}
            onChange={(evento) => setAnio(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Mes</label>
          <input
            type="number"
            min={1}
            max={12}
            required
            value={mes}
            onChange={(evento) => setMes(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Fecha inicio periodo</label>
          <input
            type="date"
            required
            value={fechaInicio}
            onChange={(evento) => setFechaInicio(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Fecha fin periodo</label>
          <input
            type="date"
            required
            value={fechaFin}
            onChange={(evento) => setFechaFin(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Inicio ventana de solicitud</label>
          <input
            type="date"
            required
            value={fechaInicioSolicitud}
            onChange={(evento) => setFechaInicioSolicitud(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-medium text-slate-700">Fin ventana de solicitud</label>
          <input
            type="date"
            required
            value={fechaFinSolicitud}
            onChange={(evento) => setFechaFinSolicitud(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
        </div>
      </div>

      <div className="mt-3 flex items-center gap-3">
        <button
          type="submit"
          disabled={enviando}
          className="rounded bg-blue-600 px-3 py-1.5 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {enviando ? "Creando..." : "Crear periodo"}
        </button>
        <button
          type="button"
          onClick={() => setAbierto(false)}
          disabled={enviando}
          className="text-xs font-medium text-slate-500 hover:text-slate-700"
        >
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
