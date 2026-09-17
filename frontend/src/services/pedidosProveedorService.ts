import { apiClient } from "./apiClient";
import type {
  AgregarDetallePedidoProveedorRequest,
  AgregarDistribucionPedidoRequest,
  CrearPedidoProveedorRequest,
  PedidoProveedor,
} from "../types/pedidoProveedor";
import type { CrearEntregaRequest, Entrega } from "../types/entrega";

// Encapsula el consumo de PedidosProveedorController (backend, docs/05-api.md §32). Exige JWT +
// PEDIDO_VER (listar/obtener/listar entregas), PEDIDO_CREAR (crear/agregar detalle/distribución),
// PEDIDO_ENVIAR, PEDIDO_CERRAR, PEDIDO_CANCELAR o ENTREGA_REGISTRAR (crear entrega anidada) —
// RN-063/ADR-066, apiClient.ts adjunta el token automáticamente. Sin alcance por empresa
// (CLAUDE.md §27): un PedidoProveedor puede consolidar necesidades de varias empresas.
export const pedidosProveedorService = {
  listar: (consolidacionId?: number | null): Promise<PedidoProveedor[]> =>
    apiClient.get<PedidoProveedor[]>(
      consolidacionId != null ? `/api/v1/pedidos-proveedor?consolidacionId=${consolidacionId}` : "/api/v1/pedidos-proveedor",
    ),

  obtener: (pedidoId: number): Promise<PedidoProveedor> =>
    apiClient.get<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}`),

  crear: (request: CrearPedidoProveedorRequest): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>("/api/v1/pedidos-proveedor", request),

  agregarDetalle: (pedidoId: number, request: AgregarDetallePedidoProveedorRequest): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}/detalles`, request),

  agregarDistribucion: (
    pedidoId: number,
    detalleId: number,
    request: AgregarDistribucionPedidoRequest,
  ): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}/detalles/${detalleId}/distribuciones`, request),

  enviar: (pedidoId: number): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}/enviar`),

  cerrar: (pedidoId: number): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}/cerrar`),

  cancelar: (pedidoId: number): Promise<PedidoProveedor> =>
    apiClient.post<PedidoProveedor>(`/api/v1/pedidos-proveedor/${pedidoId}/cancelar`),

  listarEntregas: (pedidoId: number): Promise<Entrega[]> =>
    apiClient.get<Entrega[]>(`/api/v1/pedidos-proveedor/${pedidoId}/entregas`),

  crearEntrega: (pedidoId: number, request: CrearEntregaRequest): Promise<Entrega> =>
    apiClient.post<Entrega>(`/api/v1/pedidos-proveedor/${pedidoId}/entregas`, request),
};
