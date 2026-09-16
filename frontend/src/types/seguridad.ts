// Corresponden exactamente a los DTOs reales documentados en docs/05-api.md §55/§59-§63
// (backend: Application.Organizacion.Dtos.*). RolResponse/PermisoResponse son GLOBALES (sin
// EmpresaId) — no se filtran por empresa en ningún lado.

export interface UsuarioAdmin {
  id: number;
  empresaId: number;
  nombre: string;
  apellido: string | null;
  correo: string;
  activo: boolean;
  fechaCreacion: string; // ISO 8601
  fechaActualizacion: string; // ISO 8601
}

export interface Rol {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}

export interface Permiso {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
}

// docs/05-api.md §55.2. Password viaja en texto plano únicamente en este request (HTTPS lo
// protege en tránsito) — nunca se guarda ni se vuelve a mostrar en el frontend.
export interface CrearUsuarioRequest {
  empresaId: number;
  nombre: string;
  correo: string;
  password: string;
  apellido?: string | null;
}

export interface CrearRolRequest {
  nombre: string;
  descripcion?: string | null;
}

export interface CrearPermisoRequest {
  codigo: string;
  nombre: string;
  descripcion?: string | null;
}
