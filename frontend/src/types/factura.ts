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

// docs/05-api.md §34. "estado" es el nombre real del enum C# (PascalCase) — mismo criterio ya
// usado en types/requisicion.ts.
export type FacturaEstado = "Registrada" | "Anulada";

// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.FacturaResponse (backend).
// subtotal/total son calculados por el backend (Factura.Subtotal/Total, Domain) — el frontend
// nunca debe recalcularlos, solo mostrarlos.
export interface Factura {
  id: number;
  proveedorId: number;
  pedidoProveedorId: number;
  numeroFactura: string;
  usuarioCreacionId: number;
  fechaFactura: string; // DateTime serializado como ISO 8601 por System.Text.Json.
  subtotal: number;
  impuestos: number;
  total: number;
  estado: FacturaEstado;
  observacion: string | null;
  detalles: DetalleFactura[];
}

// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.RegistrarFacturaRequest
// (backend) — sin "estado" (D-05/RN-047: una factura siempre se crea REGISTRADA, no se recibe
// del cliente) ni "fechaFactura" (FacturasController.Registrar la asigna con DateTime.UtcNow en
// el servidor).
export interface RegistrarFacturaRequest {
  proveedorId: number;
  pedidoProveedorId: number;
  numeroFactura: string;
  impuestos: number;
  observacion?: string | null;
}

// Corresponde exactamente a AuropaqPedidos.Application.Facturas.Dtos.AgregarDetalleFacturaRequest (backend).
export interface AgregarDetalleFacturaRequest {
  detallePedidoProveedorId: number;
  cantidadFacturada: number;
  precioUnitario: number;
}
