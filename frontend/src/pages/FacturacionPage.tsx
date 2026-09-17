import { useEffect, useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { SelectorCatalogo } from "../components/requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { ApiRequestError } from "../services/apiClient";
import { catalogosService } from "../services/catalogosService";
import { facturasService } from "../services/facturasService";
import { pedidosProveedorService } from "../services/pedidosProveedorService";
import type { Factura, FacturaEstado } from "../types/factura";
import type { PedidoProveedor } from "../types/pedidoProveedor";

const ESTILO_ESTADO: Record<FacturaEstado, string> = {
  Registrada: "bg-blue-100 text-blue-800",
  Anulada: "bg-red-100 text-red-700",
};

// Prioridad 3 (I3-4, docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): reemplaza la
// pantalla provisional del incremento MVP (IDs de Proveedor/Pedido ingresados a mano, porque
// GET /facturas y GET /pedidos-proveedor todavía no existían) por selección real ahora que I1-2/
// I1-4 ya están implementados. El proveedor de la factura debe coincidir con el del pedido
// (D-06/RN-039) — se toma directamente de Pedido.ProveedorId, no se vuelve a elegir.
export function FacturacionPage() {
  const [pedidoId, setPedidoId] = useState<number | null>(null);
  const [pedido, setPedido] = useState<PedidoProveedor | null>(null);
  const [facturaSeleccionada, setFacturaSeleccionada] = useState<Factura | null>(null);
  const [version, setVersion] = useState(0);

  const pedidos = useConsultaLista<PedidoProveedor>(() => pedidosProveedorService.listar(), []);
  const proveedores = useConsultaLista(() => catalogosService.listarProveedores(), []);
  const productos = useConsultaLista(() => catalogosService.listarProductos(), []);
  const facturas = useConsultaLista<Factura>(
    () => (pedidoId != null ? facturasService.listarPorPedido(pedidoId) : Promise.resolve([])),
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

  const [numeroFactura, setNumeroFactura] = useState("");
  const [impuestos, setImpuestos] = useState("0");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [detallePedidoId, setDetallePedidoId] = useState<number | null>(null);
  const [cantidadFacturada, setCantidadFacturada] = useState("");
  const [precioUnitario, setPrecioUnitario] = useState("");
  const [agregandoDetalle, setAgregandoDetalle] = useState(false);
  const [errorAgregarDetalle, setErrorAgregarDetalle] = useState<ApiRequestError | null>(null);

  const [anulando, setAnulando] = useState(false);
  const [errorAnular, setErrorAnular] = useState<ApiRequestError | null>(null);

  function nombreProducto(productoId: number): string {
    return productos.datos.find((producto) => producto.id === productoId)?.nombre ?? `Producto ${productoId}`;
  }

  function nombreProveedor(proveedorId: number): string {
    return proveedores.datos.find((proveedor) => proveedor.id === proveedorId)?.nombre ?? `Proveedor ${proveedorId}`;
  }

  function seleccionarPedido(id: number | null) {
    setPedidoId(id);
    setFacturaSeleccionada(null);
    setErrorCrear(null);
  }

  async function crearFactura(evento: React.FormEvent) {
    evento.preventDefault();
    if (pedidoId == null || pedido == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      const resultado = await facturasService.crearFactura({
        proveedorId: pedido.proveedorId,
        pedidoProveedorId: pedidoId,
        numeroFactura,
        impuestos: Number(impuestos),
      });
      setVersion((v) => v + 1);
      setFacturaSeleccionada(resultado);
      setNumeroFactura("");
      setImpuestos("0");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  async function agregarDetalle(evento: React.FormEvent) {
    evento.preventDefault();
    if (!facturaSeleccionada || detallePedidoId == null) return;

    setAgregandoDetalle(true);
    setErrorAgregarDetalle(null);
    try {
      const resultado = await facturasService.agregarDetalleFactura(facturaSeleccionada.id, {
        detallePedidoProveedorId: detallePedidoId,
        cantidadFacturada: Number(cantidadFacturada),
        precioUnitario: Number(precioUnitario),
      });
      // El backend es la fuente de verdad de subtotal/impuestos/total: se reemplaza la factura
      // completa con la respuesta recibida, nunca se recalcula en el frontend.
      setFacturaSeleccionada(resultado);
      setDetallePedidoId(null);
      setCantidadFacturada("");
      setPrecioUnitario("");
    } catch (error) {
      setErrorAgregarDetalle(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAgregandoDetalle(false);
    }
  }

  async function anularFactura() {
    if (!facturaSeleccionada) return;
    setAnulando(true);
    setErrorAnular(null);
    try {
      const resultado = await facturasService.anular(facturaSeleccionada.id);
      setFacturaSeleccionada(resultado);
      setVersion((v) => v + 1);
    } catch (error) {
      setErrorAnular(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setAnulando(false);
    }
  }

  const editableFactura = facturaSeleccionada?.estado === "Registrada";

  return (
    <main className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Facturación</h1>
      <p className="mt-1 text-sm text-slate-500">
        Una factura pertenece a un único pedido a proveedor (PedidoProveedor 1 ─── N Factura,
        RN-039). El proveedor se toma del pedido, no se vuelve a elegir.
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
            obtenerTexto={(pedido) => `Pedido ${pedido.numeroPedido} (#${pedido.id}) — ${nombreProveedor(pedido.proveedorId)}`}
            mensajeVacio="No hay pedidos registrados todavía."
          />
        </div>
      </section>

      {pedidoId != null && pedido && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Registrar factura</h2>
          <form onSubmit={crearFactura} className="mt-3 grid gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1">
              <span className="text-sm font-medium text-slate-700">Proveedor</span>
              <input value={nombreProveedor(pedido.proveedorId)} disabled className="rounded border border-slate-300 bg-slate-100 px-3 py-2 text-sm text-slate-600" />
            </label>
            <label className="flex flex-col gap-1">
              <span className="text-sm font-medium text-slate-700">Número de factura</span>
              <input
                value={numeroFactura}
                onChange={(evento) => setNumeroFactura(evento.target.value)}
                required
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </label>
            <label className="flex flex-col gap-1">
              <span className="text-sm font-medium text-slate-700">Impuestos</span>
              <input
                type="number"
                min={0}
                step="0.01"
                value={impuestos}
                onChange={(evento) => setImpuestos(evento.target.value)}
                required
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </label>
            <div className="sm:col-span-2">
              <button
                type="submit"
                disabled={creando}
                className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
              >
                {creando ? "Registrando..." : "Registrar factura"}
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

      {pedidoId != null && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-800">Facturas de este pedido</h2>
          {facturas.cargando ? (
            <p className="mt-3 text-sm text-slate-500">Cargando...</p>
          ) : facturas.error ? (
            <div className="mt-3">
              <ErrorBanner error={facturas.error} />
            </div>
          ) : facturas.datos.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">Todavía no hay ninguna factura para este pedido.</p>
          ) : (
            <ul className="mt-3 flex flex-col gap-2">
              {facturas.datos.map((factura) => (
                <li key={factura.id}>
                  <button
                    type="button"
                    onClick={() => setFacturaSeleccionada(factura)}
                    className={`flex w-full items-center justify-between rounded border px-3 py-2 text-left text-sm text-slate-900 ${
                      facturaSeleccionada?.id === factura.id ? "border-blue-500 bg-blue-50" : "border-slate-200 bg-white hover:bg-slate-50"
                    }`}
                  >
                    <span>
                      Factura {factura.numeroFactura} (#{factura.id}) — Total: {factura.total}
                    </span>
                    <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${ESTILO_ESTADO[factura.estado]}`}>
                      {factura.estado}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}

      {facturaSeleccionada && pedido && (
        <section className="mt-6 rounded-lg border border-slate-200 bg-white p-5">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-base font-semibold text-slate-800">
              Factura {facturaSeleccionada.numeroFactura} (#{facturaSeleccionada.id})
            </h2>
            <span className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTILO_ESTADO[facturaSeleccionada.estado]}`}>
              {facturaSeleccionada.estado}
            </span>
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs text-slate-500">
                  <th className="pb-2 pr-3 font-medium">Producto</th>
                  <th className="pb-2 pr-3 font-medium">Cantidad</th>
                  <th className="pb-2 pr-3 font-medium">Precio</th>
                  <th className="pb-2 font-medium">Subtotal línea</th>
                </tr>
              </thead>
              <tbody>
                {facturaSeleccionada.detalles.map((detalle) => (
                  <tr key={detalle.id} className="border-b border-slate-100">
                    <td className="py-2 pr-3 text-slate-900">{nombreProducto(detalle.productoId)}</td>
                    <td className="py-2 pr-3 text-slate-900">{detalle.cantidadFacturada}</td>
                    <td className="py-2 pr-3 text-slate-900">{detalle.precioUnitario}</td>
                    <td className="py-2 text-slate-900">{detalle.subtotal}</td>
                  </tr>
                ))}
                {facturaSeleccionada.detalles.length === 0 && (
                  <tr>
                    <td colSpan={4} className="py-3 text-sm text-slate-500">
                      Sin líneas todavía.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <dl className="mt-3 flex justify-end gap-6 text-sm">
            <div className="text-right">
              <dt className="text-xs text-slate-500">Subtotal</dt>
              <dd className="font-medium text-slate-900">{facturaSeleccionada.subtotal}</dd>
            </div>
            <div className="text-right">
              <dt className="text-xs text-slate-500">Impuestos</dt>
              <dd className="font-medium text-slate-900">{facturaSeleccionada.impuestos}</dd>
            </div>
            <div className="text-right">
              <dt className="text-xs text-slate-500">Total</dt>
              <dd className="font-semibold text-slate-900">{facturaSeleccionada.total}</dd>
            </div>
          </dl>

          {editableFactura && (
            <div className="mt-5 border-t border-slate-100 pt-4">
              <h3 className="text-sm font-semibold text-slate-700">Agregar línea</h3>
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
                  <span className="text-sm font-medium text-slate-700">Cantidad facturada</span>
                  <input
                    type="number"
                    min={1}
                    value={cantidadFacturada}
                    onChange={(evento) => setCantidadFacturada(evento.target.value)}
                    required
                    className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span className="text-sm font-medium text-slate-700">Precio unitario</span>
                  <input
                    type="number"
                    min={0}
                    step="0.01"
                    value={precioUnitario}
                    onChange={(evento) => setPrecioUnitario(evento.target.value)}
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
                    {agregandoDetalle ? "Agregando..." : "Agregar línea"}
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

          {editableFactura && (
            <div className="mt-5 flex items-center gap-3 border-t border-slate-100 pt-4">
              <button
                type="button"
                onClick={anularFactura}
                disabled={anulando}
                className="rounded border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
              >
                {anulando ? "Anulando..." : "Anular factura"}
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
