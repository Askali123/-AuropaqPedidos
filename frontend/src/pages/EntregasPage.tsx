import { useEffect, useState } from "react";
import { EntregaDetalleRow } from "../components/entregas/EntregaDetalleRow";
import { ErrorBanner } from "../components/ErrorBanner";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { entregasService } from "../services/entregasService";
import { pedidosProveedorService } from "../services/pedidosProveedorService";
import type { Entrega, EntregaEstado } from "../types/entrega";
import type { PedidoProveedor } from "../types/pedidoProveedor";

const ESTILO_ESTADO: Record<EntregaEstado, string> = {
  Registrada: "bg-blue-100 text-blue-800",
  Anulada: "bg-red-100 text-red-700",
};

// Prioridad 3 (I3-3, docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): tercera pantalla del
// tramo Fase 6-9. Una entrega solo puede registrarse contra un pedido ENVIADO o
// PARCIALMENTE_ENTREGADO (D-04/RN-046) — el backend lo valida, aquí solo se ofrece el pedido ya
// seleccionado.
export function EntregasPage() {
  const [pedidoId, setPedidoId] = useState<number | null>(null);
  const [pedido, setPedido] = useState<PedidoProveedor | null>(null);
  const [entregaSeleccionada, setEntregaSeleccionada] = useState<Entrega | null>(null);
  const [version, setVersion] = useState(0);

  const pedidos = useConsultaLista<PedidoProveedor>(() => pedidosProveedorService.listar(), []);
  const productos = useConsultaLista(() => catalogosService.listarProductos(), []);
  const entregas = useConsultaLista<Entrega>(
    () => (pedidoId != null ? pedidosProveedorService.listarEntregas(pedidoId) : Promise.resolve([])),
    [pedidoId, version],
  );

  useEffect(() => {
    if (pedidoId == null) {
      setPedido(null);
      return;
    }
    let cancelado = false;
    pedidosProveedorService.obtener(pedidoId).then((resultado) => {
      if (!cancelado) setPedido(resultado);
    });
    return () => {
      cancelado = true;
    };
  }, [pedidoId, version]);

  const [numeroRemision, setNumeroRemision] = useState("");
  const [observacion, setObservacion] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [detallePedidoId, setDetallePedidoId] = useState<number | null>(null);
  const [cantidadEntregada, setCantidadEntregada] = useState("");
  const [agregandoDetalle, setAgregandoDetalle] = useState(false);
  const [errorAgregarDetalle, setErrorAgregarDetalle] = useState<ApiRequestError | null>(null);

  const [anulando, setAnulando] = useState(false);
  const [errorAnular, setErrorAnular] = useState<ApiRequestError | null>(null);

  function nombreProducto(productoId: number): string {
    return productos.datos.find((producto) => producto.id === productoId)?.nombre ?? `Producto ${productoId}`;
  }

  function seleccionarPedido(id: number | null) {
    setPedidoId(id);
    setEntregaSeleccionada(null);
    setErrorCrear(null);
  }

  const puedeRegistrarEntrega = pedido != null && ["Enviado", "ParcialmenteEntregado"].includes(pedido.estado);

  async function crearEntrega(evento: React.FormEvent) {
    evento.preventDefault();
    if (pedidoId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      const resultado = await pedidosProveedorService.crearEntrega(pedidoId, {
        numeroRemision,
        observacion: observacion || undefined,
      });
      setVersion((v) => v + 1);
      setEntregaSeleccionada(resultado);
      setNumeroRemision("");
      setObservacion("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  async function agregarDetalle(evento: React.FormEvent) {
    evento.preventDefault();
    if (!entregaSeleccionada || detallePedidoId == null) return;

    setAgregandoDetalle(true);
    setErrorAgregarDetalle(null);
    try {
      const resultado = await entregasService.agregarDetalle(entregaSeleccionada.id, {
        detallePedidoProveedorId: detallePedidoId,
        cantidadEntregada: Number(cantidadEntregada),
      });
      setEntregaSeleccionada(resultado);
      setVersion((v) => v + 1); // el pedido puede haber cambiado de estado (ActualizarEstadoPorEntregas).
      setDetallePedidoId(null);
      setCantidadEntregada("");
    } catch (error) {
      setErrorAgregarDetalle(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAgregandoDetalle(false);
    }
  }

  async function anularEntrega() {
    if (!entregaSeleccionada) return;
    setAnulando(true);
    setErrorAnular(null);
    try {
      const resultado = await entregasService.anular(entregaSeleccionada.id);
      setEntregaSeleccionada(resultado);
      setVersion((v) => v + 1);
    } catch (error) {
      setErrorAnular(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAnulando(false);
    }
  }

  const editableEntrega = entregaSeleccionada?.estado === "Registrada";

  return (
    <main className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Entregas</h1>
      <p className="mt-1 text-sm text-slate-500">
        Un pedido puede tener varias entregas (parciales). Solo puede registrarse una entrega
        contra un pedido Enviado o Parcialmente entregado (D-04/RN-046).
      </p>

      <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
        <h2 className="text-base font-semibold text-slate-800">Pedido</h2>
        <div className="mt-3 max-w-md">
          <SelectorCatalogo
            etiqueta="Pedido a proveedor"
            datos={pedidos.datos}
            cargando={pedidos.cargando}
            error={pedidos.error}
            valorSeleccionado={pedidoId}
            onSeleccionar={seleccionarPedido}
            obtenerId={(pedido) => pedido.id}
            obtenerTexto={(pedido) => `Pedido ${pedido.numeroPedido} (#${pedido.id}) — ${pedido.estado}`}
            mensajeVacio="No hay pedidos registrados todavía."
          />
        </div>
      </section>

      {pedidoId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Registrar entrega</h2>
          {!puedeRegistrarEntrega ? (
            <p className="mt-2 text-sm text-slate-500 italic">
              Este pedido está en estado "{pedido?.estado}" — no admite entregas nuevas en esta
              etapa.
            </p>
          ) : (
            <form onSubmit={crearEntrega} className="mt-3 grid gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1">
                <span className="text-sm font-medium text-slate-700">Número de remisión</span>
                <input
                  value={numeroRemision}
                  onChange={(evento) => setNumeroRemision(evento.target.value)}
                  required
                  className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                />
              </label>
              <label className="flex flex-col gap-1">
                <span className="text-sm font-medium text-slate-700">Observación (opcional)</span>
                <input
                  value={observacion}
                  onChange={(evento) => setObservacion(evento.target.value)}
                  className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                />
              </label>
              <div className="sm:col-span-2">
                <button
                  type="submit"
                  disabled={creando}
                  className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {creando ? "Registrando..." : "Registrar entrega"}
                </button>
              </div>
            </form>
          )}
          {errorCrear && (
            <div className="mt-3">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </section>
      )}

      {pedidoId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Entregas de este pedido</h2>
          {entregas.cargando ? (
            <p className="mt-3 text-sm text-slate-500">Cargando...</p>
          ) : entregas.error ? (
            <div className="mt-3">
              <ErrorBanner error={entregas.error} />
            </div>
          ) : entregas.datos.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">Todavía no hay ninguna entrega para este pedido.</p>
          ) : (
            <ul className="mt-3 flex flex-col gap-2">
              {entregas.datos.map((entrega) => (
                <li key={entrega.id}>
                  <button
                    type="button"
                    onClick={() => setEntregaSeleccionada(entrega)}
                    className={`flex w-full items-center justify-between rounded border px-3 py-2 text-left text-sm text-slate-900 ${
                      entregaSeleccionada?.id === entrega.id ? "border-blue-500 bg-blue-50" : "border-slate-200 bg-white hover:bg-slate-50"
                    }`}
                  >
                    <span>
                      Remisión {entrega.numeroRemision} (#{entrega.id})
                    </span>
                    <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${ESTILO_ESTADO[entrega.estado]}`}>
                      {entrega.estado}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}

      {entregaSeleccionada && pedido && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-base font-semibold text-slate-800">
              Remisión {entregaSeleccionada.numeroRemision} (#{entregaSeleccionada.id})
            </h2>
            <span className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTILO_ESTADO[entregaSeleccionada.estado]}`}>
              {entregaSeleccionada.estado}
            </span>
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs text-slate-500">
                  <th className="pb-2 pr-3 font-medium">Producto</th>
                  <th className="pb-2 pr-3 font-medium">Cantidad entregada</th>
                  <th className="pb-2 font-medium">Distribuida</th>
                </tr>
              </thead>
              <tbody>
                {entregaSeleccionada.detalles.map((detalle) => (
                  <EntregaDetalleRow
                    key={detalle.id}
                    entregaId={entregaSeleccionada.id}
                    detalle={detalle}
                    nombreProducto={nombreProducto(detalle.productoId)}
                    editable={Boolean(editableEntrega)}
                    onActualizado={setEntregaSeleccionada}
                  />
                ))}
                {entregaSeleccionada.detalles.length === 0 && (
                  <tr>
                    <td colSpan={3} className="py-3 text-sm text-slate-500">
                      Todavía no se ha registrado ningún producto en esta entrega.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {editableEntrega && (
            <div className="mt-5 border-t border-slate-100 pt-4">
              <h3 className="text-sm font-semibold text-slate-700">Agregar producto entregado</h3>
              <form onSubmit={agregarDetalle} className="mt-2 grid gap-4 sm:grid-cols-3">
                <SelectorCatalogo
                  etiqueta="Producto (del pedido)"
                  datos={pedido.detalles}
                  cargando={false}
                  error={null}
                  valorSeleccionado={detallePedidoId}
                  onSeleccionar={setDetallePedidoId}
                  obtenerId={(detalle) => detalle.id}
                  obtenerTexto={(detalle) => `${nombreProducto(detalle.productoId)} (pedido: ${detalle.cantidadPedida})`}
                />
                <label className="flex flex-col gap-1">
                  <span className="text-sm font-medium text-slate-700">Cantidad entregada</span>
                  <input
                    type="number"
                    min={1}
                    value={cantidadEntregada}
                    onChange={(evento) => setCantidadEntregada(evento.target.value)}
                    required
                    className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                  />
                </label>
                <div className="sm:col-span-3">
                  <button
                    type="submit"
                    disabled={agregandoDetalle || detallePedidoId == null}
                    className="rounded bg-slate-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                  >
                    {agregandoDetalle ? "Agregando..." : "Agregar producto"}
                  </button>
                </div>
              </form>
              {errorAgregarDetalle && (
                <div className="mt-3">
                  <ErrorBanner error={errorAgregarDetalle} />
                </div>
              )}
            </div>
          )}

          {editableEntrega && (
            <div className="mt-5 flex items-center gap-3 border-t border-slate-100 pt-4">
              <button
                type="button"
                onClick={anularEntrega}
                disabled={anulando}
                className="rounded border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
              >
                {anulando ? "Anulando..." : "Anular entrega"}
              </button>
            </div>
          )}
          {errorAnular && (
            <div className="mt-3">
              <ErrorBanner error={errorAnular} />
            </div>
          )}
        </section>
      )}
    </main>
  );
}
