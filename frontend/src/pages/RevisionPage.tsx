import { useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { RequisicionPanel } from "../components/requisiciones/RequisicionPanel";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { catalogosService } from "../services/catalogosService";
import { requisicionesService } from "../services/requisicionesService";
import type { Periodo, Producto, Sede } from "../types/catalogos";
import type { Requisicion } from "../types/requisicion";

// docs/05-api.md §24.1/§54.6 punto 1: bandeja de revisión — GET /requisiciones/pendientes-revision
// (ListarRequisicionesPendientesDeRevisionUseCase) solo devuelve requisiciones en EnRevision de
// la propia Empresa del revisor (RN-058). Cada elemento ya trae el detalle completo (mismo mapper
// que Obtener), así que no hace falta una segunda consulta al abrir una fila.
//
// "Iniciar revisión" (Enviada -> EnRevision) no tiene bandeja propia — RN-060/06-seguridad.md §52
// no le asigna una ruta ni un permiso distinto de REQUISICION_APROBAR, y esta bandeja está
// deliberadamente acotada a lo que su endpoint real devuelve (EnRevision). Ese botón ya existe en
// RequisicionPanel y sigue siendo accesible desde RequisicionesPage (RN-007: una única
// Requisición por Empresa+Periodo, así que "crear o recuperar" con ese mismo Periodo recupera la
// misma Requisición aunque ya no esté en Borrador) — no se duplica aquí.
export function RevisionPage() {
  const [version, setVersion] = useState(0);
  const pendientes = useConsultaLista<Requisicion>(() => requisicionesService.listarPendientesDeRevision(), [version]);
  const productos = useConsultaLista<Producto>(() => catalogosService.listarProductos(), []);
  const periodos = useConsultaLista<Periodo>(() => catalogosService.listarPeriodos(), []);

  const [seleccionadaId, setSeleccionadaId] = useState<number | null>(null);
  const seleccionada = pendientes.datos.find((r) => r.id === seleccionadaId) ?? null;

  // Sedes de la empresa de la requisición seleccionada — igual que RequisicionesPage, solo se
  // piden cuando hay algo seleccionado (la empresa es siempre la del revisor, RN-058).
  const sedes = useConsultaLista<Sede>(
    () => (seleccionada ? catalogosService.listarSedesPorEmpresa(seleccionada.empresaId) : Promise.resolve([])),
    [seleccionada?.empresaId],
  );

  function nombrePeriodo(periodoId: number): string {
    const periodo = periodos.datos.find((p) => p.id === periodoId);
    return periodo ? `${periodo.anio}-${String(periodo.mes).padStart(2, "0")}` : `Periodo ${periodoId}`;
  }

  // Aprobar/Devolver saca la Requisición de EnRevision: ya no pertenece a esta bandeja, así que
  // se vuelve a pedir la lista completa en vez de solo reemplazar el elemento local.
  function alActualizar(actualizada: Requisicion) {
    if (actualizada.estado !== "EnRevision") {
      setSeleccionadaId(null);
      setVersion((v) => v + 1);
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Revisión de requisiciones</h1>
      <p className="mt-1 text-sm text-slate-500">
        Requisiciones en revisión de tu empresa. Ábrela para aprobarla o devolverla.
      </p>

      <div className="mt-6 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {pendientes.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : pendientes.error ? (
          <div className="p-4">
            <ErrorBanner error={pendientes.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Periodo</th>
                <th className="px-4 py-2 font-medium">Productos</th>
                <th className="px-4 py-2 font-medium">Enviada</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {pendientes.datos.map((requisicion) => (
                <tr key={requisicion.id} className="border-b border-slate-100">
                  <td className="px-4 py-2 text-slate-900">{requisicion.id}</td>
                  <td className="px-4 py-2 text-slate-900">{nombrePeriodo(requisicion.periodoId)}</td>
                  <td className="px-4 py-2 text-slate-900">{requisicion.detalles.length}</td>
                  <td className="px-4 py-2 text-slate-900">
                    {requisicion.fechaEnvio ? requisicion.fechaEnvio.slice(0, 10) : "—"}
                  </td>
                  <td className="px-4 py-2">
                    <button
                      type="button"
                      onClick={() => setSeleccionadaId(requisicion.id)}
                      className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                    >
                      Abrir
                    </button>
                  </td>
                </tr>
              ))}
              {pendientes.datos.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-3 text-sm text-slate-500">
                    No hay requisiciones en revisión.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {seleccionada && (
        <RequisicionPanel
          requisicion={seleccionada}
          productos={productos.datos}
          productosCargando={productos.cargando}
          productosError={productos.error}
          sedes={sedes.datos}
          sedesCargando={sedes.cargando}
          sedesError={sedes.error}
          onActualizado={alActualizar}
        />
      )}
    </main>
  );
}
