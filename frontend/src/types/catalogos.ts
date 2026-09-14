// Corresponden exactamente a los DTOs reales documentados en docs/05-api.md §54.5/§54.6.1
// (backend: Application.Organizacion.Dtos.EmpresaResponse/SedeResponse,
// Application.Catalogo.Dtos.ProductoResponse, Application.Periodos.Dtos.PeriodoResponse).
// Ninguno viene filtrado por "activo"/"estado": el backend no filtra, así que el frontend
// tampoco debe asumir que todo lo recibido está disponible para usarse (RN ya vigentes en el
// backend, no se duplican aquí).

export interface Empresa {
  id: number;
  nombre: string;
  activo: boolean;
}

export interface Sede {
  id: number;
  nombre: string;
  activo: boolean;
}

export interface Producto {
  id: number;
  nombre: string;
  codigoInterno: string | null;
  unidadMedidaCodigo: string;
  unidadMedidaNombre: string;
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
