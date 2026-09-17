import { apiClient } from "./apiClient";
import type { AgregarDetalleFacturaRequest, Factura, RegistrarFacturaRequest } from "../types/factura";

// Encapsula el consumo de FacturasController (backend, docs/05-api.md §34). Exige JWT +
// FACTURA_VER (listar/obtener), FACTURA_REGISTRAR (crear/agregar detalle) o FACTURA_ANULAR —
// RN-063/ADR-066, apiClient.ts adjunta el token automáticamente. Los componentes React no deben
// construir estas URLs directamente.
export const facturasService = {
  // pedidoProveedorId es obligatorio (I1-4, docs/incremento-fase-6-9-frontend-2026-09-17-1028.md):
  // el backend no expone "listar todas las facturas", solo "las de un pedido" (reutiliza
  // IFacturaRepository.ObtenerPorPedido).
  listarPorPedido: (pedidoProveedorId: number): Promise<Factura[]> =>
    apiClient.get<Factura[]>(`/api/v1/facturas?pedidoProveedorId=${pedidoProveedorId}`),

  obtener: (facturaId: number): Promise<Factura> => apiClient.get<Factura>(`/api/v1/facturas/${facturaId}`),

  crearFactura: (request: RegistrarFacturaRequest): Promise<Factura> =>
    apiClient.post<Factura>("/api/v1/facturas", request),

  agregarDetalleFactura: (facturaId: number, request: AgregarDetalleFacturaRequest): Promise<Factura> =>
    apiClient.post<Factura>(`/api/v1/facturas/${facturaId}/detalles`, request),

  anular: (facturaId: number): Promise<Factura> => apiClient.post<Factura>(`/api/v1/facturas/${facturaId}/anular`),
};
