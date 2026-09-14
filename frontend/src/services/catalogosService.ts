import { apiClient } from "./apiClient";
import type { CrearPeriodoRequest, Empresa, Periodo, Producto, Sede } from "../types/catalogos";

// Encapsula el consumo de EmpresasController/ProductosController/PeriodosController (backend,
// docs/05-api.md §54.6.1/§16.3). Empresas/Sedes/Productos son de solo lectura: ninguno admite
// creación ni requiere headers de identidad. Periodos sí admite creación (§16.3) — sin headers
// tampoco, ningún documento la condiciona a un usuario/empresa concreto. Los componentes React
// no deben construir estas URLs directamente.
export const catalogosService = {
  listarEmpresas: (): Promise<Empresa[]> => apiClient.get<Empresa[]>("/api/v1/empresas"),

  listarSedesPorEmpresa: (empresaId: number): Promise<Sede[]> =>
    apiClient.get<Sede[]>(`/api/v1/empresas/${empresaId}/sedes`),

  listarProductos: (): Promise<Producto[]> => apiClient.get<Producto[]>("/api/v1/productos"),

  listarPeriodos: (): Promise<Periodo[]> => apiClient.get<Periodo[]>("/api/v1/periodos"),

  crearPeriodo: (request: CrearPeriodoRequest): Promise<Periodo> =>
    apiClient.post<Periodo>("/api/v1/periodos", request),
};
