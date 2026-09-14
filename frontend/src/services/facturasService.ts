import { apiClient } from "./apiClient";
import type { AgregarDetalleFacturaRequest, Factura, RegistrarFacturaRequest } from "../types/factura";

// Encapsula el consumo de FacturasController (backend):
//   POST /api/v1/facturas
//   POST /api/v1/facturas/{id}/detalles
// Los componentes React no deben construir estas URLs directamente.
export const facturasService = {
  crearFactura: (request: RegistrarFacturaRequest): Promise<Factura> =>
    apiClient.post<Factura>("/api/v1/facturas", request),

  agregarDetalleFactura: (facturaId: number, request: AgregarDetalleFacturaRequest): Promise<Factura> =>
    apiClient.post<Factura>(`/api/v1/facturas/${facturaId}/detalles`, request),
};
