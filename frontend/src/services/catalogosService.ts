import { apiClient } from "./apiClient";
import type {
  ActualizarCategoriaRequest,
  ActualizarEmpresaRequest,
  ActualizarProductoProveedorRequest,
  ActualizarProductoRequest,
  ActualizarProveedorRequest,
  ActualizarSedeRequest,
  ActualizarUnidadMedidaRequest,
  Categoria,
  CrearCategoriaRequest,
  CrearEmpresaRequest,
  CrearPeriodoRequest,
  CrearProductoProveedorRequest,
  CrearProductoRequest,
  CrearProveedorRequest,
  CrearSedeRequest,
  CrearUnidadMedidaRequest,
  Empresa,
  Periodo,
  Producto,
  ProductoProveedor,
  Proveedor,
  Sede,
  UnidadMedida,
} from "../types/catalogos";

// Encapsula el consumo de EmpresasController/SedesController/CategoriasController/
// UnidadesMedidaController/ProductosController/ProveedoresController/PeriodosController
// (backend, docs/05-api.md §11-§16/§29). Todos exigen JWT desde el punto 8 (2026-09-15) —
// apiClient.ts lo adjunta automáticamente, ningún método de aquí recibe headers de identidad.
// Sin DELETE/activar-desactivar como acción separada: "Activo" viaja en el mismo PUT que el
// resto de campos (mismo criterio en los 4 recursos que lo soportan). Los componentes React no
// deben construir estas URLs directamente.
export const catalogosService = {
  listarEmpresas: (): Promise<Empresa[]> => apiClient.get<Empresa[]>("/api/v1/empresas"),

  crearEmpresa: (request: CrearEmpresaRequest): Promise<Empresa> =>
    apiClient.post<Empresa>("/api/v1/empresas", request),

  actualizarEmpresa: (empresaId: number, request: ActualizarEmpresaRequest): Promise<Empresa> =>
    apiClient.put<Empresa>(`/api/v1/empresas/${empresaId}`, request),

  listarSedesPorEmpresa: (empresaId: number): Promise<Sede[]> =>
    apiClient.get<Sede[]>(`/api/v1/empresas/${empresaId}/sedes`),

  crearSede: (empresaId: number, request: CrearSedeRequest): Promise<Sede> =>
    apiClient.post<Sede>(`/api/v1/empresas/${empresaId}/sedes`, request),

  // docs/05-api.md §12.3: ruta propia /sedes/{id}, no anidada bajo empresas/{empresaId}.
  actualizarSede: (sedeId: number, request: ActualizarSedeRequest): Promise<Sede> =>
    apiClient.put<Sede>(`/api/v1/sedes/${sedeId}`, request),

  listarCategorias: (): Promise<Categoria[]> => apiClient.get<Categoria[]>("/api/v1/categorias"),

  crearCategoria: (request: CrearCategoriaRequest): Promise<Categoria> =>
    apiClient.post<Categoria>("/api/v1/categorias", request),

  actualizarCategoria: (categoriaId: number, request: ActualizarCategoriaRequest): Promise<Categoria> =>
    apiClient.put<Categoria>(`/api/v1/categorias/${categoriaId}`, request),

  listarUnidadesMedida: (): Promise<UnidadMedida[]> => apiClient.get<UnidadMedida[]>("/api/v1/unidades-medida"),

  crearUnidadMedida: (request: CrearUnidadMedidaRequest): Promise<UnidadMedida> =>
    apiClient.post<UnidadMedida>("/api/v1/unidades-medida", request),

  actualizarUnidadMedida: (unidadMedidaId: number, request: ActualizarUnidadMedidaRequest): Promise<UnidadMedida> =>
    apiClient.put<UnidadMedida>(`/api/v1/unidades-medida/${unidadMedidaId}`, request),

  listarProductos: (): Promise<Producto[]> => apiClient.get<Producto[]>("/api/v1/productos"),

  crearProducto: (request: CrearProductoRequest): Promise<Producto> =>
    apiClient.post<Producto>("/api/v1/productos", request),

  actualizarProducto: (productoId: number, request: ActualizarProductoRequest): Promise<Producto> =>
    apiClient.put<Producto>(`/api/v1/productos/${productoId}`, request),

  listarProveedores: (): Promise<Proveedor[]> => apiClient.get<Proveedor[]>("/api/v1/proveedores"),

  crearProveedor: (request: CrearProveedorRequest): Promise<Proveedor> =>
    apiClient.post<Proveedor>("/api/v1/proveedores", request),

  actualizarProveedor: (proveedorId: number, request: ActualizarProveedorRequest): Promise<Proveedor> =>
    apiClient.put<Proveedor>(`/api/v1/proveedores/${proveedorId}`, request),

  listarPeriodos: (): Promise<Periodo[]> => apiClient.get<Periodo[]>("/api/v1/periodos"),

  crearPeriodo: (request: CrearPeriodoRequest): Promise<Periodo> =>
    apiClient.post<Periodo>("/api/v1/periodos", request),

  // docs/05-api.md §30 (TASK-019). Misma relación consultada desde ambos lados: por producto
  // (crear/listar/editar) y por proveedor (solo listar — la creación siempre parte de un
  // Producto, no hay un segundo formulario de alta desde Proveedores).
  listarProveedoresDeProducto: (productoId: number): Promise<ProductoProveedor[]> =>
    apiClient.get<ProductoProveedor[]>(`/api/v1/productos/${productoId}/proveedores`),

  asociarProveedorAProducto: (productoId: number, request: CrearProductoProveedorRequest): Promise<ProductoProveedor> =>
    apiClient.post<ProductoProveedor>(`/api/v1/productos/${productoId}/proveedores`, request),

  actualizarProductoProveedor: (
    productoId: number,
    relacionId: number,
    request: ActualizarProductoProveedorRequest,
  ): Promise<ProductoProveedor> =>
    apiClient.put<ProductoProveedor>(`/api/v1/productos/${productoId}/proveedores/${relacionId}`, request),

  listarProductosDeProveedor: (proveedorId: number): Promise<ProductoProveedor[]> =>
    apiClient.get<ProductoProveedor[]>(`/api/v1/proveedores/${proveedorId}/productos`),
};
