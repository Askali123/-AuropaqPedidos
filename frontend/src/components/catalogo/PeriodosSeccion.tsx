import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { CrearPeriodoForm } from "../requisiciones/CrearPeriodoForm";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { catalogosService } from "../../services/catalogosService";

// docs/05-api.md §16. Solo lista + alta (reutiliza CrearPeriodoForm, ya usado en
// RequisicionesPage): sin PUT/activar-desactivar, no existe en el backend (§16, Periodo no lo
// documenta) — no se inventa uno aquí.
export function PeriodosSeccion() {
  const [version, setVersion] = useState(0);
  const periodos = useConsultaLista(() => catalogosService.listarPeriodos(), [version]);

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-slate-800">Periodos</h2>
        <CrearPeriodoForm onCreado={() => setVersion((v) => v + 1)} />
      </div>

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {periodos.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : periodos.error ? (
          <div className="p-4">
            <ErrorBanner error={periodos.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Año</th>
                <th className="px-4 py-2 font-medium">Mes</th>
                <th className="px-4 py-2 font-medium">Ventana de solicitud</th>
                <th className="px-4 py-2 font-medium">Estado</th>
              </tr>
            </thead>
            <tbody>
              {periodos.datos.map((periodo) => (
                <tr key={periodo.id} className="border-b border-slate-100">
                  <td className="px-4 py-2 text-slate-900">{periodo.id}</td>
                  <td className="px-4 py-2 text-slate-900">{periodo.anio}</td>
                  <td className="px-4 py-2 text-slate-900">{periodo.mes}</td>
                  <td className="px-4 py-2 text-slate-900">
                    {periodo.fechaInicioSolicitud.slice(0, 10)} — {periodo.fechaFinSolicitud.slice(0, 10)}
                  </td>
                  <td className="px-4 py-2 text-slate-900">{periodo.estado}</td>
                </tr>
              ))}
              {periodos.datos.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-3 text-sm text-slate-500">
                    No hay periodos registrados.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
