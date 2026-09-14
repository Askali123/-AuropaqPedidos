import { useState } from "react";
import { ApiRequestError } from "../services/apiClient";
import { facturasService } from "../services/facturasService";
import type { Factura } from "../types/factura";
import "./FacturacionPage.css";

// Incremento MVP frontend ↔ backend: primer vertical slice real (React → Api → Application →
// Domain → Infrastructure → SQL Server) para Facturación (TASK-046). No implementa
// autenticación, selección dinámica de Proveedor/Pedido (esos GET todavía no existen —
// docs/05-api.md §31/§32 los marca conceptuales) ni el resto de acciones de Factura.
export function FacturacionPage() {
  // Paso 1 — Crear factura. IDs de Proveedor/PedidoProveedor se ingresan manualmente:
  // PROVISIONAL, únicamente porque no existen todavía endpoints GET para listarlos.
  const [proveedorId, setProveedorId] = useState("");
  const [pedidoProveedorId, setPedidoProveedorId] = useState("");
  const [numeroFactura, setNumeroFactura] = useState("");
  const [impuestos, setImpuestos] = useState("0");
  // 06/§7 de la propuesta de negocio: sin ciclo de vida de Estado en este incremento; no hay
  // valores documentados para elegir, así que no se expone como campo editable.
  const estado = "REGISTRADA";
  const [observacion, setObservacion] = useState("");

  const [crearLoading, setCrearLoading] = useState(false);
  const [crearError, setCrearError] = useState<ApiRequestError | null>(null);
  const [factura, setFactura] = useState<Factura | null>(null);

  // Paso 2 — Agregar detalle.
  const [detallePedidoProveedorId, setDetallePedidoProveedorId] = useState("");
  const [cantidadFacturada, setCantidadFacturada] = useState("");
  const [precioUnitario, setPrecioUnitario] = useState("");

  const [detalleLoading, setDetalleLoading] = useState(false);
  const [detalleError, setDetalleError] = useState<ApiRequestError | null>(null);

  async function crearFactura(evento: React.FormEvent) {
    evento.preventDefault();
    setCrearLoading(true);
    setCrearError(null);

    try {
      const resultado = await facturasService.crearFactura({
        proveedorId: Number(proveedorId),
        pedidoProveedorId: Number(pedidoProveedorId),
        numeroFactura,
        impuestos: Number(impuestos),
        estado,
        observacion: observacion || undefined,
      });
      setFactura(resultado);
    } catch (error) {
      setCrearError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setFactura(null);
    } finally {
      setCrearLoading(false);
    }
  }

  async function agregarDetalle(evento: React.FormEvent) {
    evento.preventDefault();
    if (!factura) return;

    setDetalleLoading(true);
    setDetalleError(null);

    try {
      const resultado = await facturasService.agregarDetalleFactura(factura.id, {
        detallePedidoProveedorId: Number(detallePedidoProveedorId),
        cantidadFacturada: Number(cantidadFacturada),
        precioUnitario: Number(precioUnitario),
      });
      // El backend es la fuente de verdad de subtotal/impuestos/total: se reemplaza la
      // factura completa con la respuesta recibida, nunca se recalcula en el frontend.
      setFactura(resultado);
      setDetallePedidoProveedorId("");
      setCantidadFacturada("");
      setPrecioUnitario("");
    } catch (error) {
      setDetalleError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setDetalleLoading(false);
    }
  }

  return (
    <main className="facturacion">
      <h1>Facturación</h1>
      <p className="nota-provisional">
        MVP técnico (TASK-046). Proveedor y Pedido se identifican por Id ingresado manualmente:
        <strong> provisional</strong>, porque todavía no existen endpoints para listarlos
        (docs/05-api.md §31/§32, conceptuales).
      </p>

      <section className="tarjeta">
        <h2>Crear factura</h2>
        <form onSubmit={crearFactura}>
          <label>
            Proveedor (Id)
            <input value={proveedorId} onChange={(e) => setProveedorId(e.target.value)} required inputMode="numeric" />
          </label>
          <label>
            Pedido a proveedor (Id)
            <input value={pedidoProveedorId} onChange={(e) => setPedidoProveedorId(e.target.value)} required inputMode="numeric" />
          </label>
          <label>
            Número de factura
            <input value={numeroFactura} onChange={(e) => setNumeroFactura(e.target.value)} required />
          </label>
          <label>
            Fecha
            <input value="Asignada por el servidor al registrar (no se envía desde el frontend)" disabled />
          </label>
          <label>
            Estado
            <input value={`${estado} (fijo: sin ciclo de vida definido todavía)`} disabled />
          </label>
          <label>
            Impuestos
            <input value={impuestos} onChange={(e) => setImpuestos(e.target.value)} required inputMode="decimal" />
          </label>
          <label>
            Observación
            <input value={observacion} onChange={(e) => setObservacion(e.target.value)} />
          </label>

          <button type="submit" disabled={crearLoading}>
            {crearLoading ? "Creando..." : "Crear factura"}
          </button>
        </form>

        {crearError && <ErrorBanner error={crearError} />}

        {factura && (
          <div className="resultado-ok">
            Factura creada — Id: <strong>{factura.id}</strong> (Estado: {factura.estado})
          </div>
        )}
      </section>

      {factura && (
        <section className="tarjeta">
          <h2>Agregar detalle</h2>
          <form onSubmit={agregarDetalle}>
            <label>
              Detalle de pedido (Id)
              <input
                value={detallePedidoProveedorId}
                onChange={(e) => setDetallePedidoProveedorId(e.target.value)}
                required
                inputMode="numeric"
              />
            </label>
            <label>
              Cantidad facturada
              <input value={cantidadFacturada} onChange={(e) => setCantidadFacturada(e.target.value)} required inputMode="numeric" />
            </label>
            <label>
              Precio unitario
              <input value={precioUnitario} onChange={(e) => setPrecioUnitario(e.target.value)} required inputMode="decimal" />
            </label>

            <button type="submit" disabled={detalleLoading}>
              {detalleLoading ? "Agregando..." : "Agregar detalle"}
            </button>
          </form>

          {detalleError && <ErrorBanner error={detalleError} />}
        </section>
      )}

      {factura && (
        <section className="tarjeta">
          <h2>Resultado</h2>
          <table className="detalles">
            <thead>
              <tr>
                <th>Detalle pedido</th>
                <th>Producto</th>
                <th>Cantidad</th>
                <th>Precio</th>
                <th>Subtotal línea</th>
              </tr>
            </thead>
            <tbody>
              {factura.detalles.map((detalle) => (
                <tr key={detalle.id}>
                  <td>{detalle.detallePedidoProveedorId}</td>
                  <td>{detalle.productoId}</td>
                  <td>{detalle.cantidadFacturada}</td>
                  <td>{detalle.precioUnitario}</td>
                  <td>{detalle.subtotal}</td>
                </tr>
              ))}
              {factura.detalles.length === 0 && (
                <tr>
                  <td colSpan={5}>Sin líneas todavía.</td>
                </tr>
              )}
            </tbody>
          </table>

          <dl className="totales">
            <dt>Subtotal</dt>
            <dd>{factura.subtotal}</dd>
            <dt>Impuestos</dt>
            <dd>{factura.impuestos}</dd>
            <dt>Total</dt>
            <dd>
              <strong>{factura.total}</strong>
            </dd>
          </dl>
        </section>
      )}
    </main>
  );
}

function ErrorBanner({ error }: { error: ApiRequestError }) {
  return (
    <div className="resultado-error" role="alert">
      <strong>
        {error.status || "Sin respuesta"} — {error.code}
      </strong>
      <p>{error.message}</p>
      {error.details.length > 0 && (
        <ul>
          {error.details.map((detalle) => (
            <li key={detalle}>{detalle}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
