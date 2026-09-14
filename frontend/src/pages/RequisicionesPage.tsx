import { useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { RequisicionPanel } from "../components/requisiciones/RequisicionPanel";
import { CrearPeriodoForm } from "../components/requisiciones/CrearPeriodoForm";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { requisicionesService } from "../services/requisicionesService";
import type { Empresa, Periodo, Producto, Sede } from "../types/catalogos";
import type { Requisicion } from "../types/requisicion";

// Séptima etapa del Frontend de Requisiciones — vertical slice real
// (React → Api → Application → Domain → Infrastructure → SQL Server). Además de crear/recuperar
// una Requisición (BORRADOR), gestionar sus detalles/distribuciones, enviarla, iniciar revisión
// y aprobarla (etapas anteriores), ahora permite devolverla (docs/05-api.md §54.4 fila 12) — ver
// el botón "Devolver requisición" en RequisicionPanel, visible solo cuando Estado es
// "EnRevision" (misma condición que Aprobar: son las dos transiciones posibles desde ahí). A
// diferencia de Aprobar, el motivo es obligatorio (Requisicion.Devolver(), Domain) — el botón se
// deshabilita si está vacío, sin reimplementar la regla, solo evitando un 422 previsible. RN-018
// tampoco define ningún rol concreto para devolver (mismo vacío ya documentado para
// iniciar-revision/aprobar) — no se inventa ningún control de permisos nuevo.
//
// Todavía NO implementa (instrucción explícita de esta etapa): consolidación, necesidad de
// compra, pedido a proveedor, ni ninguna funcionalidad posterior. El botón "Continuar" sigue sin
// funcionalidad real: marca dónde continúa la siguiente etapa sin inventar ese comportamiento.
export function RequisicionesPage() {
  const [empresaId, setEmpresaId] = useState<number | null>(null);
  const [periodoId, setPeriodoId] = useState<number | null>(null);

  // Placeholder temporal (docs/05-api.md §54.1): no existe login todavía, así que no hay forma
  // real de saber quién es el usuario. Mismo criterio que FacturacionPage con Proveedor/Pedido:
  // se ingresa manualmente en vez de inventar una sesión que no existe. Cuando exista
  // autenticación real, este es el único campo de esta pantalla que debe eliminarse.
  const [usuarioId, setUsuarioId] = useState("1");

  const [requisicion, setRequisicion] = useState<Requisicion | null>(null);
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const empresas = useConsultaLista<Empresa>(() => catalogosService.listarEmpresas(), []);
  // periodosVersion fuerza a useConsultaLista a volver a pedir la lista tras crear un Periodo
  // (CrearPeriodoForm) — mismo hook, sin duplicar su lógica de carga/error.
  const [periodosVersion, setPeriodosVersion] = useState(0);
  const periodos = useConsultaLista<Periodo>(() => catalogosService.listarPeriodos(), [periodosVersion]);
  const productos = useConsultaLista<Producto>(() => catalogosService.listarProductos(), []);
  // RN-002: una sede pertenece a exactamente una empresa. Sin empresa elegida no hay nada que
  // consultar (ni se llama al endpoint); al cambiar de empresa, la lista se vuelve a pedir sola
  // porque empresaId forma parte de las dependencias del hook.
  const sedes = useConsultaLista<Sede>(
    () => (empresaId != null ? catalogosService.listarSedesPorEmpresa(empresaId) : Promise.resolve([])),
    [empresaId],
  );

  // Empresa y Periodo identifican qué Requisición se está trabajando (RN-007). Si cambian
  // después de haber creado/recuperado una, la Requisición cargada ya no corresponde a la
  // selección actual: se limpia para no mostrar datos de una combinación distinta a la elegida.
  function seleccionarEmpresa(id: number | null) {
    setEmpresaId(id);
    setRequisicion(null);
    setErrorCrear(null);
  }

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
    if (empresaId == null || periodoId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      const resultado = await requisicionesService.iniciarOContinuar(empresaId, Number(usuarioId), { periodoId });
      setRequisicion(resultado);
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setRequisicion(null);
    } finally {
      setCreando(false);
    }
  }

  const empresaSeleccionada = empresas.datos.find((e) => e.id === empresaId) ?? null;
  const puedeCrear = empresaId != null && periodoId != null && usuarioId.trim() !== "" && !creando;

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
          <SelectorCatalogo
            etiqueta="Empresa"
            datos={empresas.datos}
            cargando={empresas.cargando}
            error={empresas.error}
            valorSeleccionado={empresaId}
            onSeleccionar={seleccionarEmpresa}
            obtenerId={(empresa) => empresa.id}
            obtenerTexto={(empresa) => empresa.nombre + (empresa.activo ? "" : " (inactiva)")}
            mensajeVacio="No hay empresas registradas."
          />
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
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-slate-700">Usuario (Id)</label>
            <input
              type="number"
              min={1}
              value={usuarioId}
              onChange={(evento) => setUsuarioId(evento.target.value)}
              className="w-24 rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
            />
            <span className="text-xs text-slate-400">Provisional: no hay sesión de usuario todavía.</span>
          </div>

          <button
            type="button"
            onClick={crearOContinuar}
            disabled={!puedeCrear}
            className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Consultando..." : "Crear o recuperar requisición"}
          </button>
        </div>
        {empresaId != null && periodoId != null && (
          <p className="mt-2 text-xs text-slate-400">
            Empresa: {empresaSeleccionada?.nombre ?? empresaId} — Periodo elegido arriba. Cambiar cualquiera de los
            dos iniciará una nueva búsqueda de requisición.
          </p>
        )}

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
            usuarioId={usuarioId}
            onActualizado={setRequisicion}
          />

          <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
            <h2 className="text-base font-semibold text-slate-800">Siguiente etapa</h2>
            <p className="mt-1 text-xs text-slate-500">
              Consolidación, necesidad de compra y pedido a proveedor son etapas posteriores, no
              implementadas todavía en esta pantalla.
            </p>
            <button
              type="button"
              disabled
              title="Próxima etapa: consolidación (no implementado todavía)"
              className="mt-4 cursor-not-allowed rounded bg-slate-300 px-4 py-2 text-sm font-medium text-slate-600"
            >
              Continuar (próxima etapa)
            </button>
          </section>
        </>
      )}
    </main>
  );
}
