// Corresponden exactamente a AuropaqPedidos.Application.PedidosProveedor.Dtos (backend,
// docs/05-api.md §32). "estado" es el nombre real del enum C# (PascalCase) — mismo criterio ya
// usado en types/requisicion.ts.
export type PedidoProveedorEstado = "Borrador" | "Enviado" | "ParcialmenteEntregado" | "Entregado" | "Cerrado" | "Cancelado";

export interface DistribucionPedido {
  id: number;
  sedeId: number;
  cantidad: number;
}

export interface DetallePedidoProveedor {
  id: number;
  productoId: number;
  detalleConsolidacionId: number | null;
  cantidadNecesaria: number;
  cantidadPedida: number;
  precioUnitario: number | null;
  // TASK-105 (docs/2026-09-18-auditoria-dominio-roles-frontend.md): fotografía del código que el
  // proveedor de este pedido usaba para el producto al momento de agregar el detalle (resuelto
  // automáticamente por el backend contra ProductoProveedor) — null si no existía esa relación.
  codigoProveedorUtilizado: string | null;
  distribuciones: DistribucionPedido[];
}

export interface PedidoProveedor {
  id: number;
  consolidacionId: number;
  proveedorId: number;
  numeroPedido: string;
  usuarioCreacionId: number;
  fechaPedido: string; // ISO 8601
  fechaEntregaEstimada: string | null;
  estado: PedidoProveedorEstado;
  observacion: string | null;
  detalles: DetallePedidoProveedor[];
}

// docs/05-api.md §32 (CrearPedidoProveedorRequest, Application/PedidosProveedor/Dtos).
// ProveedorId/NumeroPedido explícitos del llamador (D-01/D-09) — sin selección automática de
// proveedor ni generación automática de número.
export interface CrearPedidoProveedorRequest {
  consolidacionId: number;
  proveedorId: number;
  numeroPedido: string;
  fechaEntregaEstimada?: string | null;
  observacion?: string | null;
}

// docs/05-api.md §32 (AgregarDetallePedidoProveedorRequest). CantidadPedida se exige explícita
// (RN-031): nunca se copia automáticamente de CantidadNecesaria.
export interface AgregarDetallePedidoProveedorRequest {
  detalleConsolidacionId: number;
  cantidadPedida: number;
  precioUnitario?: number | null;
}

// docs/05-api.md §32 (AgregarDistribucionPedidoRequest, Api/Controllers/Dtos).
export interface AgregarDistribucionPedidoRequest {
  sedeId: number;
  cantidad: number;
}
