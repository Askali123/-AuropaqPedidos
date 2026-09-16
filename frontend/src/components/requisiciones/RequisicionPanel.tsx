import { useState } from "react";
import { AgregarDetalleForm } from "./AgregarDetalleForm";
import { DetalleRequisicionRow } from "./DetalleRequisicionRow";
import { ErrorBanner } from "../ErrorBanner";
import { ApiRequestError } from "../../services/apiClient";
import { requisicionesService } from "../../services/requisicionesService";
import { ESTADOS_EDITABLES } from "../../types/requisicion";
import type { Producto, Sede } from "../../types/catalogos";
import type { Requisicion } from "../../types/requisicion";

interface RequisicionPanelProps {
  requisicion: Requisicion;
  productos: Producto[];
  productosCargando: boolean;
  productosError: ApiRequestError | null;
  sedes: Sede[];
  sedesCargando: boolean;
  sedesError: ApiRequestError | null;
  onActualizado: (requisicion: Requisicion) => void;
}

const ESTILO_ESTADO: Record<string, string> = {
  Borrador: "bg-amber-100 text-amber-800",
  Devuelta: "bg-amber-100 text-amber-800",
  Enviada: "bg-blue-100 text-blue-800",
  EnRevision: "bg-blue-100 text-blue-800",
  Aprobada: "bg-green-100 text-green-800",
};

// Panel de "detalle de la requisición": una vez creada/recuperada, este componente concentra
// todo lo que esta etapa permite hacer con ella (agregar/editar/eliminar detalle, distribuir
// cada detalle entre sedes, guardar borrador, enviar, iniciar revisión, aprobar, devolver). No
// conoce cómo se creó la Requisicion — solo la recibe y avisa al padre cuando cambia
// (docs/05-api.md §54.4, filas 2-12).
export function RequisicionPanel({
  requisicion,
  productos,
  productosCargando,
  productosError,
  sedes,
  sedesCargando,
  sedesError,
  onActualizado,
}: RequisicionPanelProps) {
  const [guardando, setGuardando] = useState(false);
  const [errorGuardar, setErrorGuardar] = useState<ApiRequestError | null>(null);
  const [guardadoOk, setGuardadoOk] = useState(false);

  const [enviando, setEnviando] = useState(false);
  const [errorEnviar, setErrorEnviar] = useState<ApiRequestError | null>(null);
  const [enviadoOk, setEnviadoOk] = useState(false);

  const [iniciandoRevision, setIniciandoRevision] = useState(false);
  const [errorIniciarRevision, setErrorIniciarRevision] = useState<ApiRequestError | null>(null);
  const [revisionIniciadaOk, setRevisionIniciadaOk] = useState(false);

  const [observacionAprobar, setObservacionAprobar] = useState("");
  const [aprobando, setAprobando] = useState(false);
  const [errorAprobar, setErrorAprobar] = useState<ApiRequestError | null>(null);
  const [aprobadoOk, setAprobadoOk] = useState(false);

  const [motivoDevolver, setMotivoDevolver] = useState("");
  const [devolviendo, setDevolviendo] = useState(false);
  const [errorDevolver, setErrorDevolver] = useState<ApiRequestError | null>(null);
  const [devueltoOk, setDevueltoOk] = useState(false);

  // docs/05-api.md §54.3: EsEditable (Domain) solo es verdadero en Borrador/Devuelta. No se
  // inventa un criterio propio — se usa exactamente esta lista. Mismo criterio determina cuándo
  // mostrar "Enviar requisición": el backend rechaza el envío con 422 en cualquier otro estado
  // (Requisicion.AsegurarEditable, Domain), así que no tiene sentido ofrecer el botón entonces.
  const editable = ESTADOS_EDITABLES.includes(requisicion.estado);

  // docs/05-api.md §54.4 fila 10 / §54.3: Requisicion.IniciarRevision() (Domain) exige
  // Estado == Enviada; cualquier otro estado responde 422. "Enviada" es el único valor real que
  // usa el backend (§54.3, PascalCase) — no se compara contra la versión conceptual en mayúsculas.
  const puedeIniciarRevision = requisicion.estado === "Enviada";

  // docs/05-api.md §54.4 fila 11 / §54.3: Requisicion.Aprobar() (Domain) exige
  // Estado == EnRevision; cualquier otro estado responde 422. RN-017 no define ningún rol
  // concreto ("el usuario autorizado puede aprobar") — mismo vacío ya documentado en §54.7 para
  // iniciar-revision, no se inventa ningún control de permisos nuevo aquí.
  const puedeAprobar = requisicion.estado === "EnRevision";

  // docs/05-api.md §54.4 fila 12 / §54.3: Requisicion.Devolver() (Domain) exige
  // Estado == EnRevision (misma condición que Aprobar; son las dos transiciones posibles desde
  // ese estado). RN-018 tampoco define ningún rol concreto — mismo criterio de estado, sin
  // inventar control de permisos.
  const puedeDevolver = requisicion.estado === "EnRevision";

  // Motivo obligatorio, no vacío/blanco (Requisicion.Devolver(), Domain) — misma condición que
  // ya valida el backend; aquí solo se usa para deshabilitar el botón, no se reimplementa nada.
  const motivoDevolverValido = motivoDevolver.trim() !== "";

  // Evita que Guardar borrador, Enviar, Iniciar revisión, Aprobar y Devolver se disparen a la vez
  // sobre la misma Requisicion (todas mutan el mismo agregado en el backend); ninguna se invalida
  // mutuamente por regla de negocio, es solo una precaución de UI.
  const procesando = guardando || enviando || iniciandoRevision || aprobando || devolviendo;

  function nombreProducto(productoId: number): string {
    return productos.find((producto) => producto.id === productoId)?.nombre ?? `Producto ${productoId}`;
  }

  async function guardarBorrador() {
    setGuardando(true);
    setErrorGuardar(null);
    setGuardadoOk(false);
    try {
      const resultado = await requisicionesService.guardarBorrador(requisicion.id);
      onActualizado(resultado);
      setGuardadoOk(true);
    } catch (error) {
      setErrorGuardar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setGuardando(false);
    }
  }

  // docs/05-api.md §54.4, fila 9. El backend vuelve a validar todo (detalles, empresa activa,
  // ventana de solicitud, distribución completa por detalle — Requisicion.Enviar(), Domain): el
  // frontend no repite ninguna de esas condiciones, solo muestra el error que llegue si falla.
  async function enviarRequisicion() {
    setEnviando(true);
    setErrorEnviar(null);
    setEnviadoOk(false);
    try {
      const resultado = await requisicionesService.enviar(requisicion.id);
      onActualizado(resultado);
      setEnviadoOk(true);
    } catch (error) {
      setErrorEnviar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setEnviando(false);
    }
  }

  // docs/05-api.md §54.4, fila 10. Único chequeo del backend: Estado == Enviada
  // (Requisicion.IniciarRevision(), Domain) — el frontend no repite esa condición más que para
  // decidir si muestra el botón; cualquier otra causa de rechazo la reporta el backend.
  async function iniciarRevision() {
    setIniciandoRevision(true);
    setErrorIniciarRevision(null);
    setRevisionIniciadaOk(false);
    try {
      const resultado = await requisicionesService.iniciarRevision(requisicion.id);
      onActualizado(resultado);
      setRevisionIniciadaOk(true);
    } catch (error) {
      setErrorIniciarRevision(
        error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)),
      );
    } finally {
      setIniciandoRevision(false);
    }
  }

  // docs/05-api.md §54.4, fila 11. Único chequeo del backend: Estado == EnRevision
  // (Requisicion.Aprobar(), Domain) — el frontend no repite esa condición más que para decidir
  // si muestra el botón; "observacion" es opcional, se envía tal cual (vacío -> undefined, igual
  // que AgregarDetalleForm) y no se valida su contenido en el cliente.
  async function aprobarRequisicion() {
    setAprobando(true);
    setErrorAprobar(null);
    setAprobadoOk(false);
    try {
      const resultado = await requisicionesService.aprobar(requisicion.id, {
        observacion: observacionAprobar || undefined,
      });
      onActualizado(resultado);
      setAprobadoOk(true);
      setObservacionAprobar("");
    } catch (error) {
      setErrorAprobar(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAprobando(false);
    }
  }

  // docs/05-api.md §54.4, fila 12. Único chequeo adicional del backend respecto a Aprobar:
  // "motivo" no puede estar vacío/blanco (Requisicion.Devolver(), Domain) — se deshabilita el
  // botón como ayuda de UX (motivoDevolverValido), pero el backend sigue siendo quien decide.
  async function devolverRequisicion() {
    setDevolviendo(true);
    setErrorDevolver(null);
    setDevueltoOk(false);
    try {
      const resultado = await requisicionesService.devolver(requisicion.id, {
        motivo: motivoDevolver,
      });
      onActualizado(resultado);
      setDevueltoOk(true);
      setMotivoDevolver("");
    } catch (error) {
      setErrorDevolver(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setDevolviendo(false);
    }
  }

  return (
    <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-base font-semibold text-slate-800">Requisición #{requisicion.id}</h2>
        <span
          className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTILO_ESTADO[requisicion.estado] ?? "bg-slate-100 text-slate-700"}`}
        >
          {requisicion.estado}
        </span>
      </div>

      {!editable && (
        <p className="mt-2 text-sm text-slate-500 italic">
          Esta requisición está en estado "{requisicion.estado}" y ya no puede modificarse en esta etapa.
        </p>
      )}

      <div className="mt-4 overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-b border-slate-200 text-xs text-slate-500">
              <th className="pb-2 pr-3 font-medium">Producto</th>
              <th className="pb-2 pr-3 font-medium">Cantidad solicitada</th>
              <th className="pb-2 pr-3 font-medium">Observación</th>
              <th className="pb-2 pr-3 font-medium">Distribuida</th>
              <th className="pb-2 font-medium">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {requisicion.detalles.map((detalle) => (
              <DetalleRequisicionRow
                key={detalle.id}
                requisicionId={requisicion.id}
                detalle={detalle}
                nombreProducto={nombreProducto(detalle.productoId)}
                editable={editable}
                sedes={sedes}
                sedesCargando={sedesCargando}
                sedesError={sedesError}
                onActualizado={onActualizado}
              />
            ))}
            {requisicion.detalles.length === 0 && (
              <tr>
                <td colSpan={5} className="py-3 text-sm text-slate-500">
                  Todavía no se ha agregado ningún producto.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {editable && (
        <div className="mt-5 border-t border-slate-100 pt-4">
          <h3 className="text-sm font-semibold text-slate-700">Agregar producto</h3>
          <div className="mt-2">
            <AgregarDetalleForm
              requisicionId={requisicion.id}
              productos={productos}
              productosCargando={productosCargando}
              productosError={productosError}
              onAgregado={onActualizado}
            />
          </div>
        </div>
      )}

      <div className="mt-5 flex flex-wrap items-center gap-3 border-t border-slate-100 pt-4">
        <button
          type="button"
          onClick={guardarBorrador}
          disabled={procesando}
          className="rounded bg-slate-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          {guardando ? "Guardando..." : "Guardar borrador"}
        </button>

        {editable && (
          <button
            type="button"
            onClick={enviarRequisicion}
            disabled={procesando}
            className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {enviando ? "Enviando..." : "Enviar requisición"}
          </button>
        )}

        {puedeIniciarRevision && (
          <button
            type="button"
            onClick={iniciarRevision}
            disabled={procesando}
            className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {iniciandoRevision ? "Iniciando revisión..." : "Iniciar revisión"}
          </button>
        )}

        {puedeAprobar && (
          <>
            <input
              type="text"
              value={observacionAprobar}
              onChange={(evento) => setObservacionAprobar(evento.target.value)}
              placeholder="Observación (opcional)"
              disabled={procesando}
              className="w-56 rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 disabled:opacity-50"
            />
            <button
              type="button"
              onClick={aprobarRequisicion}
              disabled={procesando}
              className="rounded bg-green-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
            >
              {aprobando ? "Aprobando..." : "Aprobar requisición"}
            </button>
          </>
        )}

        {puedeDevolver && (
          <>
            <input
              type="text"
              value={motivoDevolver}
              onChange={(evento) => setMotivoDevolver(evento.target.value)}
              placeholder="Motivo (obligatorio)"
              disabled={procesando}
              className="w-56 rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 disabled:opacity-50"
            />
            <button
              type="button"
              onClick={devolverRequisicion}
              disabled={procesando || !motivoDevolverValido}
              className="rounded bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
            >
              {devolviendo ? "Devolviendo..." : "Devolver requisición"}
            </button>
          </>
        )}

        {guardadoOk && <span className="text-sm text-green-700">Borrador guardado.</span>}
        {enviadoOk && <span className="text-sm text-green-700">Requisición enviada.</span>}
        {revisionIniciadaOk && <span className="text-sm text-green-700">Revisión iniciada.</span>}
        {aprobadoOk && <span className="text-sm text-green-700">Requisición aprobada.</span>}
        {devueltoOk && <span className="text-sm text-green-700">Requisición devuelta.</span>}
      </div>
      {errorGuardar && (
        <div className="mt-2">
          <ErrorBanner error={errorGuardar} />
        </div>
      )}
      {errorEnviar && (
        <div className="mt-2">
          <ErrorBanner error={errorEnviar} />
        </div>
      )}
      {errorIniciarRevision && (
        <div className="mt-2">
          <ErrorBanner error={errorIniciarRevision} />
        </div>
      )}
      {errorAprobar && (
        <div className="mt-2">
          <ErrorBanner error={errorAprobar} />
        </div>
      )}
      {errorDevolver && (
        <div className="mt-2">
          <ErrorBanner error={errorDevolver} />
        </div>
      )}
    </section>
  );
}
