import { useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { RequisicionPanel } from "../components/requisiciones/RequisicionPanel";
import { CrearPeriodoForm } from "../components/requisiciones/CrearPeriodoForm";
import { useAuth } from "../auth/AuthContext";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { requisicionesService } from "../services/requisicionesService";
import type { Periodo, Producto, Sede } from "../types/catalogos";
import type { Requisicion } from "../types/requisicion";

// Octava etapa del Frontend de Requisiciones — ahora sobre sesión real (docs/05-api.md §56):
// Empresa y Usuario ya no se ingresan a mano, se derivan del JWT (useAuth). El backend también
// dejó de aceptar X-Usuario-Id/X-Empresa-Id en este Controller (punto 8, 2026-09-15) — enviarlos
// ya no tendría ningún efecto.
export function RequisicionesPage() {
  const { usuario } = useAuth();
  const [periodoId, setPeriodoId] = useState<number | null>(null);

  const [requisicion, setRequisicion] = useState<Requisicion | null>(null);
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  // periodosVersion fuerza a useConsultaLista a volver a pedir la lista tras crear un Periodo
  // (CrearPeriodoForm) — mismo hook, sin duplicar su lógica de carga/error.
  const [periodosVersion, setPeriodosVersion] = useState(0);
  const periodos = useConsultaLista<Periodo>(() => catalogosService.listarPeriodos(), [periodosVersion]);
  const productos = useConsultaLista<Producto>(() => catalogosService.listarProductos(), []);
  // RN-002: una sede pertenece a exactamente una empresa — la del usuario autenticado.
  const sedes = useConsultaLista<Sede>(
    () => (usuario ? catalogosService.listarSedesPorEmpresa(usuario.empresaId) : Promise.resolve([])),
    [usuario?.empresaId],
  );

  // El Periodo identifica, junto con la Empresa del usuario (RN-007), qué Requisición se está
  // trabajando. Si cambia después de haber creado/recuperado una, la Requisición cargada ya no
  // corresponde a la selección actual: se limpia para no mostrar datos de una combinación
  // distinta a la elegida.
  function seleccionarPeriodo(id: number | null) {
    setPeriodoId(id);
    setRequisicion(null);
    setErrorCrear(null);
  }

  // Tras crear un Periodo (CrearPeriodoForm), se recarga la lista y se selecciona el nuevo de
  // una vez — mismo efecto que si el usuario lo hubiera elegido manualmente del selector.
  function periodoCreado(periodo: Periodo) {
    setPeriodosVersion((version) => version + 1);
    seleccionarPeriodo(periodo.id);
  }

  async function crearOContinuar() {
    if (periodoId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      const resultado = await requisicionesService.iniciarOContinuar({ periodoId });
      setRequisicion(resultado);
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setRequisicion(null);
    } finally {
      setCreando(false);
    }
  }

  const puedeCrear = periodoId != null && !creando;

  return (
    <main className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Requisiciones</h1>
      <p className="mt-1 text-sm text-slate-500">
        Conectada a la Api real. Crea o recupera una requisición en Borrador, gestiona sus
        detalles (agregar, editar cantidad, eliminar) y la distribución de cada uno entre sedes,
        guarda el borrador, y permite enviarla, iniciar su revisión, aprobarla o devolverla.
      </p>

      <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
        <h2 className="text-base font-semibold text-slate-800">Requisición</h2>
        <div className="mt-3 grid gap-4 sm:grid-cols-2">
          <div>
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
            <div className="mt-1">
              <CrearPeriodoForm onCreado={periodoCreado} />
            </div>
          </div>
        </div>

        <div className="mt-4 flex flex-wrap items-end gap-4">
          <button
            type="button"
            onClick={crearOContinuar}
            disabled={!puedeCrear}
            className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Consultando..." : "Crear o recuperar requisición"}
          </button>
        </div>

        {errorCrear && (
          <div className="mt-3">
            <ErrorBanner error={errorCrear} />
          </div>
        )}
      </section>

      {requisicion && (
        <>
          <RequisicionPanel
            requisicion={requisicion}
            productos={productos.datos}
            productosCargando={productos.cargando}
            productosError={productos.error}
            sedes={sedes.datos}
            sedesCargando={sedes.cargando}
            sedesError={sedes.error}
            onActualizado={setRequisicion}
          />

          <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
            <h2 className="text-base font-semibold text-slate-800">Siguiente etapa</h2>
            <p className="mt-1 text-xs text-slate-500">
              Una vez enviada y aprobada, esta requisición se incluye en la siguiente
              consolidación del periodo — ver "Revisión" (aprobar/devolver) y luego
              "Consolidación", "Pedidos a proveedor", "Entregas" y "Facturación" en el menú.
            </p>
          </section>
        </>
      )}
    </main>
  );
}
