import { useEffect, useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { PedidoDetalleRow } from "../components/pedidosProveedor/PedidoDetalleRow";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { consolidacionesService } from "../services/consolidacionesService";
import { pedidosProveedorService } from "../services/pedidosProveedorService";
import type { Consolidacion } from "../types/consolidacion";
import type { PedidoProveedor, PedidoProveedorEstado } from "../types/pedidoProveedor";

const ESTILO_ESTADO: Record<PedidoProveedorEstado, string> = {
  Borrador: "bg-amber-100 text-amber-800",
  Enviado: "bg-blue-100 text-blue-800",
  ParcialmenteEntregado: "bg-blue-100 text-blue-800",
  Entregado: "bg-green-100 text-green-800",
  Cerrado: "bg-slate-200 text-slate-700",
  Cancelado: "bg-red-100 text-red-700",
};

// Prioridad 3 (I3-2, docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): segunda pantalla del
// tramo Fase 6-9. Un PedidoProveedor se crea a partir de una Consolidación existente y no
// pertenece a una única Empresa (CLAUDE.md §27) — por eso Proveedor/Sede se eligen del catálogo
// global, no del catálogo de "mi empresa" como en Requisiciones.
export function PedidosProveedorPage() {
  const [consolidacionId, setConsolidacionId] = useState<number | null>(null);
  const [pedidoSeleccionado, setPedidoSeleccionado] = useState<PedidoProveedor | null>(null);
  const [version, setVersion] = useState(0);

  const consolidaciones = useConsultaLista<Consolidacion>(() => consolidacionesService.listar(), []);
  const proveedores = useConsultaLista(() => catalogosService.listarProveedores(), []);
  const productos = useConsultaLista(() => catalogosService.listarProductos(), []);
  const pedidos = useConsultaLista<PedidoProveedor>(
    () => (consolidacionId != null ? pedidosProveedorService.listar(consolidacionId) : Promise.resolve([])),
    [consolidacionId, version],
  );

  // Se necesita el detalle completo de la Consolidación (no solo la fila de la lista) para saber
  // qué DetalleConsolidacion todavía no se agregó a ningún DetallePedidoProveedor.
  const [consolidacion, setConsolidacion] = useState<Consolidacion | null>(null);
  useEffect(() => {
    if (consolidacionId == null) {
      setConsolidacion(null);
      return;
    }
    let cancelado = false;
    consolidacionesService.obtener(consolidacionId).then((resultado) => {
      if (!cancelado) setConsolidacion(resultado);
    });
    return () => {
      cancelado = true;
    };
  }, [consolidacionId]);

  const [proveedorId, setProveedorId] = useState<number | null>(null);
  const [numeroPedido, setNumeroPedido] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [detalleConsolidacionId, setDetalleConsolidacionId] = useState<number | null>(null);
  const [cantidadPedida, setCantidadPedida] = useState("");
  const [precioUnitario, setPrecioUnitario] = useState("");
  const [agregandoDetalle, setAgregandoDetalle] = useState(false);
  const [errorAgregarDetalle, setErrorAgregarDetalle] = useState<ApiRequestError | null>(null);

  const [transicionando, setTransicionando] = useState(false);
  const [errorTransicion, setErrorTransicion] = useState<ApiRequestError | null>(null);

  function nombreProducto(productoId: number): string {
    return productos.datos.find((producto) => producto.id === productoId)?.nombre ?? `Producto ${productoId}`;
  }

  function seleccionarConsolidacion(id: number | null) {
    setConsolidacionId(id);
    setPedidoSeleccionado(null);
    setErrorCrear(null);
  }

  async function crearPedido(evento: React.FormEvent) {
    evento.preventDefault();
    if (consolidacionId == null || proveedorId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      const resultado = await pedidosProveedorService.crear({ consolidacionId, proveedorId, numeroPedido });
      setVersion((v) => v + 1);
      setPedidoSeleccionado(resultado);
      setNumeroPedido("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  async function agregarDetalle(evento: React.FormEvent) {
    evento.preventDefault();
    if (!pedidoSeleccionado || detalleConsolidacionId == null) return;

    setAgregandoDetalle(true);
    setErrorAgregarDetalle(null);
    try {
      const resultado = await pedidosProveedorService.agregarDetalle(pedidoSeleccionado.id, {
        detalleConsolidacionId,
        cantidadPedida: Number(cantidadPedida),
        precioUnitario: precioUnitario ? Number(precioUnitario) : undefined,
      });
      setPedidoSeleccionado(resultado);
      setDetalleConsolidacionId(null);
      setCantidadPedida("");
      setPrecioUnitario("");
    } catch (error) {
      setErrorAgregarDetalle(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAgregandoDetalle(false);
    }
  }

  async function transicionar(accion: "enviar" | "cerrar" | "cancelar") {
    if (!pedidoSeleccionado) return;
    setTransicionando(true);
    setErrorTransicion(null);
    try {
      const resultado = await pedidosProveedorService[accion](pedidoSeleccionado.id);
      setPedidoSeleccionado(resultado);
      setVersion((v) => v + 1);
    } catch (error) {
      setErrorTransicion(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setTransicionando(false);
    }
  }

  const detallesConsolidacionDisponibles = (consolidacion?.detalles ?? []).filter(
    (detalleConsolidacion) =>
      !pedidoSeleccionado?.detalles.some((detallePedido) => detallePedido.detalleConsolidacionId === detalleConsolidacion.id),
  );

  const editable = pedidoSeleccionado?.estado === "Borrador";

  return (
    <main className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Pedidos a proveedor</h1>
      <p className="mt-1 text-sm text-slate-500">
        Un pedido se crea a partir de una consolidación existente y agrupa lo que se le pide a un
        único proveedor. No pertenece a una sola empresa (CLAUDE.md §27).
      </p>

      <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
        <h2 className="text-base font-semibold text-slate-800">Consolidación</h2>
        <div className="mt-3 max-w-md">
          <SelectorCatalogo
            etiqueta="Consolidación"
            datos={consolidaciones.datos}
            cargando={consolidaciones.cargando}
            error={consolidaciones.error}
            valorSeleccionado={consolidacionId}
            onSeleccionar={seleccionarConsolidacion}
            obtenerId={(consolidacion) => consolidacion.id}
            obtenerTexto={(consolidacion) => `Consolidación #${consolidacion.id} — ${consolidacion.estado}`}
            mensajeVacio="No hay consolidaciones registradas todavía."
          />
        </div>
      </section>

      {consolidacionId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Crear pedido</h2>
          <form onSubmit={crearPedido} className="mt-3 grid gap-4 sm:grid-cols-2">
            <SelectorCatalogo
              etiqueta="Proveedor"
              datos={proveedores.datos}
              cargando={proveedores.cargando}
              error={proveedores.error}
              valorSeleccionado={proveedorId}
              onSeleccionar={setProveedorId}
              obtenerId={(proveedor) => proveedor.id}
              obtenerTexto={(proveedor) => proveedor.nombre}
              mensajeVacio="No hay proveedores registrados."
            />
            <label className="flex flex-col gap-1">
              <span className="text-sm font-medium text-slate-700">Número de pedido</span>
              <input
                value={numeroPedido}
                onChange={(evento) => setNumeroPedido(evento.target.value)}
                required
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </label>
            <div className="sm:col-span-2">
              <button
                type="submit"
                disabled={creando || proveedorId == null}
                className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
              >
                {creando ? "Creando..." : "Crear pedido"}
              </button>
            </div>
          </form>
          {errorCrear && (
            <div className="mt-3">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </section>
      )}

      {consolidacionId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Pedidos de esta consolidación</h2>
          {pedidos.cargando ? (
            <p className="mt-3 text-sm text-slate-500">Cargando...</p>
          ) : pedidos.error ? (
            <div className="mt-3">
              <ErrorBanner error={pedidos.error} />
            </div>
          ) : pedidos.datos.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">Todavía no hay ningún pedido para esta consolidación.</p>
          ) : (
            <ul className="mt-3 flex flex-col gap-2">
              {pedidos.datos.map((pedido) => (
                <li key={pedido.id}>
                  <button
                    type="button"
                    onClick={() => setPedidoSeleccionado(pedido)}
                    className={`flex w-full items-center justify-between rounded border px-3 py-2 text-left text-sm text-slate-900 ${
                      pedidoSeleccionado?.id === pedido.id ? "border-blue-500 bg-blue-50" : "border-slate-200 bg-white hover:bg-slate-50"
                    }`}
                  >
                    <span>
                      Pedido {pedido.numeroPedido} (#{pedido.id})
                    </span>
                    <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${ESTILO_ESTADO[pedido.estado]}`}>
                      {pedido.estado}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}

      {pedidoSeleccionado && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-base font-semibold text-slate-800">
              Pedido {pedidoSeleccionado.numeroPedido} (#{pedidoSeleccionado.id})
            </h2>
            <span className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTILO_ESTADO[pedidoSeleccionado.estado]}`}>
              {pedidoSeleccionado.estado}
            </span>
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs text-slate-500">
                  <th className="pb-2 pr-3 font-medium">Producto</th>
                  <th className="pb-2 pr-3 font-medium">Código proveedor</th>
                  <th className="pb-2 pr-3 font-medium">Cantidad necesaria</th>
                  <th className="pb-2 pr-3 font-medium">Cantidad pedida</th>
                  <th className="pb-2 pr-3 font-medium">Precio unitario</th>
                  <th className="pb-2 font-medium">Distribuida</th>
                </tr>
              </thead>
              <tbody>
                {pedidoSeleccionado.detalles.map((detalle) => (
                  <PedidoDetalleRow
                    key={detalle.id}
                    pedidoId={pedidoSeleccionado.id}
                    detalle={detalle}
                    nombreProducto={nombreProducto(detalle.productoId)}
                    editable={Boolean(editable)}
                    onActualizado={setPedidoSeleccionado}
                  />
                ))}
                {pedidoSeleccionado.detalles.length === 0 && (
                  <tr>
                    <td colSpan={6} className="py-3 text-sm text-slate-500">
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
              {detallesConsolidacionDisponibles.length === 0 ? (
                <p className="mt-2 text-sm text-slate-500">
                  Todos los productos de la consolidación ya se agregaron a este pedido.
                </p>
              ) : (
                <form onSubmit={agregarDetalle} className="mt-2 grid gap-4 sm:grid-cols-3">
                  <SelectorCatalogo
                    etiqueta="Producto (de la consolidación)"
                    datos={detallesConsolidacionDisponibles}
                    cargando={false}
                    error={null}
                    valorSeleccionado={detalleConsolidacionId}
                    onSeleccionar={setDetalleConsolidacionId}
                    obtenerId={(detalle) => detalle.id}
                    obtenerTexto={(detalle) => `${nombreProducto(detalle.productoId)} (necesario: ${detalle.cantidadNecesaria})`}
                  />
                  <label className="flex flex-col gap-1">
                    <span className="text-sm font-medium text-slate-700">Cantidad pedida</span>
                    <input
                      type="number"
                      min={1}
                      value={cantidadPedida}
                      onChange={(evento) => setCantidadPedida(evento.target.value)}
                      required
                      className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                    />
                  </label>
                  <label className="flex flex-col gap-1">
                    <span className="text-sm font-medium text-slate-700">Precio unitario (opcional)</span>
                    <input
                      type="number"
                      min={0}
                      step="0.01"
                      value={precioUnitario}
                      onChange={(evento) => setPrecioUnitario(evento.target.value)}
                      className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                    />
                  </label>
                  <div className="sm:col-span-3">
                    <button
                      type="submit"
                      disabled={agregandoDetalle || detalleConsolidacionId == null}
                      className="rounded bg-slate-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                    >
                      {agregandoDetalle ? "Agregando..." : "Agregar producto"}
                    </button>
                  </div>
                </form>
              )}
              {errorAgregarDetalle && (
                <div className="mt-3">
                  <ErrorBanner error={errorAgregarDetalle} />
                </div>
              )}
            </div>
          )}

          <div className="mt-5 flex flex-wrap items-center gap-3 border-t border-slate-100 pt-4">
            {editable && (
              <button
                type="button"
                onClick={() => transicionar("enviar")}
                disabled={transicionando}
                className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              >
                Enviar pedido
              </button>
            )}
            {pedidoSeleccionado.estado === "Entregado" && (
              <button
                type="button"
                onClick={() => transicionar("cerrar")}
                disabled={transicionando}
                className="rounded bg-green-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              >
                Cerrar pedido
              </button>
            )}
            {["Borrador", "Enviado", "ParcialmenteEntregado"].includes(pedidoSeleccionado.estado) && (
              <button
                type="button"
                onClick={() => transicionar("cancelar")}
                disabled={transicionando}
                className="rounded border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
              >
                Cancelar pedido
              </button>
            )}
          </div>
          {errorTransicion && (
            <div className="mt-3">
              <ErrorBanner error={errorTransicion} />
            </div>
          )}

          <p className="mt-4 text-xs text-slate-500">
            Siguiente etapa: registrar entregas para este pedido en la pantalla "Entregas" (Pedido
            #{pedidoSeleccionado.id}), y facturas en "Facturación".
          </p>
        </section>
      )}
    </main>
  );
}

