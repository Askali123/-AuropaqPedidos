// Corresponde al contrato real documentado en docs/05-api.md §56.1 (backend: AuthController /
// LoginUseCase). El JWT no trae claims de roles ni permisos todavía (RN-059/060 se validan en el
// backend vía [Authorize]; el frontend no reconstruye esa matriz aquí).
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
