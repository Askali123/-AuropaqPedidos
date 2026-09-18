// Corresponde al contrato real documentado en docs/05-api.md §56.1 (backend: AuthController /
// LoginUseCase). El JWT no trae claims de roles ni permisos (decisión mantenida, ver §56.3): el
// Frontend los obtiene por separado vía GET /auth/mis-permisos (TASK-102, ver AuthContext.tsx),
// consultados en vivo en cada sesión en vez de copiados dentro del token.
export interface UsuarioAutenticado {
  id: number;
  empresaId: number;
  nombre: string;
  apellido: string | null;
  correo: string;
  activo: boolean;
  fechaCreacion: string; // ISO 8601
  fechaActualizacion: string; // ISO 8601
}

export interface LoginRequest {
  correo: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  fechaExpiracion: string; // ISO 8601
  usuario: UsuarioAutenticado;
}

// docs/05-api.md §56.3 (PermisoResponse, GET /auth/mis-permisos).
export interface Permiso {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
}
