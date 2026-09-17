import { useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { consolidacionesService } from "../services/consolidacionesService";
import type { Periodo } from "../types/catalogos";
import type { Consolidacion } from "../types/consolidacion";

// Prioridad 3 (I3-1, docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): primera pantalla del
// tramo Fase 6-9 en el Frontend. Consolidación agrupa la necesidad de todas las requisiciones
// APROBADAS de un Periodo que todavía no hayan participado en otra consolidación (A2,
// CrearConsolidacionUseCase) — no depende de una sola Empresa ni Sede (CLAUDE.md §25/§27).
export function ConsolidacionPage() {
  const [periodoId, setPeriodoId] = useState<number | null>(null);
  const [consolidacionSeleccionada, setConsolidacionSeleccionada] = useState<Consolidacion | null>(null);

  const [version, setVersion] = useState(0);
  const periodos = useConsultaLista<Periodo>(() => catalogosService.listarPeriodos(), []);
  const consolidaciones = useConsultaLista<Consolidacion>(
    () => (periodoId != null ? consolidacionesService.listar(periodoId) : Promise.resolve([])),
    [periodoId, version],
  );
  const productos = useConsultaLista(() => catalogosService.listarProductos(), []);

  const [generando, setGenerando] = useState(false);
  const [errorGenerar, setErrorGenerar] = useState<ApiRequestError | null>(null);

  function nombreProducto(productoId: number): string {
    return productos.datos.find((producto) => producto.id === productoId)?.nombre ?? `Producto ${productoId}`;
  }

  function seleccionarPeriodo(id: number | null) {
    setPeriodoId(id);
    setConsolidacionSeleccionada(null);
    setErrorGenerar(null);
  }

  // TASK-036, RN-027/RN-028: la consolidación toma como fuente las requisiciones APROBADAS del
  // periodo, sin modificarlas — el backend decide qué incluye (A2: excluye lo ya consolidado
  // antes), el frontend solo dispara la acción.
  async function generarConsolidacion() {
    if (periodoId == null) return;
    setGenerando(true);
    setErrorGenerar(null);
    try {
      const resultado = await consolidacionesService.crear({ periodoId, estado: "GENERADA" });
      setVersion((v) => v + 1);
      setConsolidacionSeleccionada(resultado);
    } catch (error) {
      setErrorGenerar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setGenerando(false);
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Consolidación</h1>
      <p className="mt-1 text-sm text-slate-500">
        Agrupa la necesidad de todas las requisiciones aprobadas de un periodo que todavía no se
        hayan consolidado. Una consolidación nunca modifica las requisiciones originales
        (CLAUDE.md §25).
      </p>

      <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
        <h2 className="text-base font-semibold text-slate-800">Periodo</h2>
        <div className="mt-3 max-w-xs">
          <SelectorCatalogo
            etiqueta="Periodo"
            datos={periodos.datos}
            cargando={periodos.cargando}
            error={periodos.error}
            valorSeleccionado={periodoId}
            onSeleccionar={seleccionarPeriodo}
            obtenerId={(periodo) => periodo.id}
            obtenerTexto={(periodo) => `${periodo.anio}-${String(periodo.mes).padStart(2, "0")} (${periodo.estado})`}
            mensajeVacio="No hay periodos registrados."
          />
        </div>

        {periodoId != null && (
          <div className="mt-4">
            <button
              type="button"
              onClick={generarConsolidacion}
              disabled={generando}
              className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
            >
              {generando ? "Generando..." : "Generar consolidación"}
            </button>
            {errorGenerar && (
              <div className="mt-3">
                <ErrorBanner error={errorGenerar} />
              </div>
            )}
          </div>
        )}
      </section>

      {periodoId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Consolidaciones del periodo</h2>

          {consolidaciones.cargando ? (
            <p className="mt-3 text-sm text-slate-500">Cargando...</p>
          ) : consolidaciones.error ? (
            <div className="mt-3">
              <ErrorBanner error={consolidaciones.error} />
            </div>
          ) : consolidaciones.datos.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">Todavía no hay ninguna consolidación para este periodo.</p>
          ) : (
            <ul className="mt-3 flex flex-col gap-2">
              {consolidaciones.datos.map((consolidacion) => (
                <li key={consolidacion.id}>
                  <button
                    type="button"
                    onClick={() => setConsolidacionSeleccionada(consolidacion)}
                    className={`w-full rounded border px-3 py-2 text-left text-sm text-slate-900 ${
                      consolidacionSeleccionada?.id === consolidacion.id
                        ? "border-blue-500 bg-blue-50"
                        : "border-slate-200 bg-white hover:bg-slate-50"
                    }`}
                  >
                    Consolidación #{consolidacion.id} — {consolidacion.estado} — {consolidacion.detalles.length} producto(s)
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}

      {consolidacionSeleccionada && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Consolidación #{consolidacionSeleccionada.id}</h2>
          <p className="mt-1 text-xs text-slate-500">
            Creada el {new Date(consolidacionSeleccionada.fechaCreacion).toLocaleString()}
            {consolidacionSeleccionada.observacion ? ` — ${consolidacionSeleccionada.observacion}` : ""}
          </p>

          <div className="mt-4 overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs text-slate-500">
                  <th className="pb-2 pr-3 font-medium">Producto</th>
                  <th className="pb-2 pr-3 font-medium">Cantidad necesaria</th>
                  <th className="pb-2 font-medium">Requisiciones de origen</th>
                </tr>
              </thead>
              <tbody>
                {consolidacionSeleccionada.detalles.map((detalle) => (
                  <tr key={detalle.id} className="border-b border-slate-100">
                    <td className="py-2 pr-3 text-slate-900">{nombreProducto(detalle.productoId)}</td>
                    <td className="py-2 pr-3 text-slate-900">{detalle.cantidadNecesaria}</td>
                    <td className="py-2 text-xs text-slate-500">
                      {detalle.asignaciones.map((asignacion) => `#${asignacion.detalleRequisicionId} (${asignacion.cantidad})`).join(", ")}
                    </td>
                  </tr>
                ))}
                {consolidacionSeleccionada.detalles.length === 0 && (
                  <tr>
                    <td colSpan={3} className="py-3 text-sm text-slate-500">
                      Sin productos consolidados.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <p className="mt-4 text-xs text-slate-500">
            Siguiente etapa: crear un pedido a proveedor a partir de esta consolidación, en la
            pantalla "Pedidos a proveedor" (Consolidación #{consolidacionSeleccionada.id}).
          </p>
        </section>
      )}
    </main>
  );
}
