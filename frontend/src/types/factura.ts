// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.DetalleFacturaResponse
// (backend). Nombres verificados contra el JSON real devuelto por la Api.
export interface DetalleFactura {
  id: number;
  detallePedidoProveedorId: number;
  productoId: number;
  cantidadFacturada: number;
  precioUnitario: number;
  subtotal: number;
}

// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.FacturaResponse (backend).
// subtotal/total son calculados por el backend (Factura.Subtotal/Total, Domain) — el frontend
// nunca debe recalcularlos, solo mostrarlos.
export interface Factura {
  id: number;
  proveedorId: number;
  pedidoProveedorId: number;
  numeroFactura: string;
  fechaFactura: string; // DateTime serializado como ISO 8601 por System.Text.Json.
  subtotal: number;
  impuestos: number;
  total: number;
  estado: string;
  observacion: string | null;
  detalles: DetalleFactura[];
}

// Corresponde a AuropaqPedidos.Application.Facturas.Dtos.RegistrarFacturaRequest (backend).
//
// DIFERENCIA DOCUMENTADA: este request NO incluye FechaFactura, aunque conceptualmente sea un
// dato de la factura. FacturasController.Registrar (Api) asigna FechaFactura con
// DateTime.UtcNow en el servidor y no la recibe del cliente — no existe forma de enviarla desde
// el frontend en este incremento. La pantalla de "Crear factura" lo indica explícitamente.
export interface RegistrarFacturaRequest {
  proveedorId: number;
  pedidoProveedorId: number;
  numeroFactura: string;
  impuestos: number;
  estado: string;
  observacion?: string;
}

// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.AgregarDetalleFacturaRequest (backend).
export interface AgregarDetalleFacturaRequest {
  detallePedidoProveedorId: number;
  cantidadFacturada: number;
  precioUnitario: number;
}
