// Corresponden exactamente a AuropaqPedidos.Application.Entregas.Dtos (backend,
// docs/05-api.md §33). "estado" es el nombre real del enum C# (PascalCase).
export type EntregaEstado = "Registrada" | "Anulada";

export interface DistribucionEntrega {
  id: number;
  sedeId: number;
  cantidad: number;
  // Fotografía histórica de la Sede en el momento de registrar (RN-035/ADR-019): puede diferir
  // de los datos actuales de la Sede si esta se modificó después.
  direccionEntrega: string | null;
  ciudadEntrega: string | null;
  contactoEntrega: string | null;
}

export interface DetalleEntrega {
  id: number;
  detallePedidoProveedorId: number;
  productoId: number;
  cantidadEntregada: number;
  distribuciones: DistribucionEntrega[];
}

export interface Entrega {
  id: number;
  pedidoProveedorId: number;
  usuarioCreacionId: number;
  fechaEntrega: string; // ISO 8601
  numeroRemision: string;
  estado: EntregaEstado;
  observacion: string | null;
  detalles: DetalleEntrega[];
}

// docs/05-api.md §33 (CrearEntregaHttpRequest, Api/Controllers/Dtos) — el PedidoProveedorId va
// en la ruta (POST /pedidos-proveedor/{id}/entregas), no en este cuerpo.
export interface CrearEntregaRequest {
  numeroRemision: string;
  observacion?: string | null;
}

// docs/05-api.md §33 (AgregarDetalleEntregaRequest, Application/Entregas/Dtos).
export interface AgregarDetalleEntregaRequest {
  detallePedidoProveedorId: number;
  cantidadEntregada: number;
}

// docs/05-api.md §33 (AgregarDistribucionEntregaRequest, Api/Controllers/Dtos).
export interface AgregarDistribucionEntregaRequest {
  sedeId: number;
  cantidad: number;
}
