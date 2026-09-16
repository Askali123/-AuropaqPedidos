// Corresponden exactamente a los DTOs reales documentados en docs/05-api.md §11-§16/§29/§54.5
// (backend: Application.Organizacion.Dtos.*, Application.Catalogo.Dtos.*,
// Application.Periodos.Dtos.PeriodoResponse). Ninguno viene filtrado por "activo"/"estado": el
// backend no filtra, así que el frontend tampoco debe asumir que todo lo recibido está
// disponible para usarse (RN ya vigentes en el backend, no se duplican aquí).

export interface Empresa {
  id: number;
  nombre: string;
  activo: boolean;
}

// docs/05-api.md §11.3/§11.4 (CrearEmpresaRequest/ActualizarEmpresaRequest). "Activo" solo existe
// en Actualizar: una Empresa nueva siempre nace activa (CrearEmpresaUseCase).
export interface CrearEmpresaRequest {
  nombre: string;
  nit?: string | null;
}

export interface ActualizarEmpresaRequest {
  nombre: string;
  nit: string | null;
  activo: boolean;
}

export interface Sede {
  id: number;
  nombre: string;
  direccion: string | null;
  ciudad: string | null;
  departamento: string | null;
  telefono: string | null;
  contacto: string | null;
  activo: boolean;
}

// docs/05-api.md §12.2/§12.3. Igual que Empresa: una Sede nueva siempre nace activa.
export interface CrearSedeRequest {
  nombre: string;
  direccion?: string | null;
  ciudad?: string | null;
  departamento?: string | null;
  telefono?: string | null;
  contacto?: string | null;
}

export interface ActualizarSedeRequest {
  nombre: string;
  direccion: string | null;
  ciudad: string | null;
  departamento: string | null;
  telefono: string | null;
  contacto: string | null;
  activo: boolean;
}

export interface Categoria {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}

export interface CrearCategoriaRequest {
  nombre: string;
  descripcion?: string | null;
}

export interface ActualizarCategoriaRequest {
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}

export interface UnidadMedida {
  id: number;
  codigo: string;
  nombre: string;
  activo: boolean;
}

export interface CrearUnidadMedidaRequest {
  codigo: string;
  nombre: string;
}

export interface ActualizarUnidadMedidaRequest {
  codigo: string;
  nombre: string;
  activo: boolean;
}

export interface Producto {
  id: number;
  nombre: string;
  codigoInterno: string | null;
  descripcion: string | null;
  categoriaId: number;
  categoriaNombre: string;
  unidadMedidaId: number;
  unidadMedidaCodigo: string;
  unidadMedidaNombre: string;
  activo: boolean;
}

// docs/05-api.md §13.3/§13.4. CategoriaId/UnidadMedidaId obligatorios (RN-021, invariante de
// Domain — Producto exige ambos).
export interface CrearProductoRequest {
  nombre: string;
  categoriaId: number;
  unidadMedidaId: number;
  codigoInterno?: string | null;
  descripcion?: string | null;
}

export interface ActualizarProductoRequest {
  nombre: string;
  categoriaId: number;
  unidadMedidaId: number;
  codigoInterno: string | null;
  descripcion: string | null;
  activo: boolean;
}

export interface Proveedor {
  id: number;
  nombre: string;
  nit: string | null;
  contacto: string | null;
  telefono: string | null;
  correo: string | null;
  activo: boolean;
}

export interface CrearProveedorRequest {
  nombre: string;
  nit?: string | null;
  contacto?: string | null;
  telefono?: string | null;
  correo?: string | null;
}

export interface ActualizarProveedorRequest {
  nombre: string;
  nit: string | null;
  contacto: string | null;
  telefono: string | null;
  correo: string | null;
  activo: boolean;
}

export interface Periodo {
  id: number;
  anio: number;
  mes: number;
  fechaInicio: string; // DateTime serializado como ISO 8601 por System.Text.Json.
  fechaFin: string;
  fechaInicioSolicitud: string;
  fechaFinSolicitud: string;
  estado: string;
}

// docs/05-api.md §16.3 (CrearPeriodoRequest, Application/Periodos/Dtos). "Estado" no va en el
// body — el backend lo fija en "ABIERTO" al crear (decisión explícita del usuario, ver
// CrearPeriodoUseCase). Las fechas viajan como "yyyy-MM-dd" (inputs type="date" del formulario).
export interface CrearPeriodoRequest {
  anio: number;
  mes: number;
  fechaInicio: string;
  fechaFin: string;
  fechaInicioSolicitud: string;
  fechaFinSolicitud: string;
}
