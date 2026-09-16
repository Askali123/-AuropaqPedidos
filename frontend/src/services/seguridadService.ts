import { apiClient } from "./apiClient";
import type {
  CrearPermisoRequest,
  CrearRolRequest,
  CrearUsuarioRequest,
  Permiso,
  Rol,
  UsuarioAdmin,
} from "../types/seguridad";

// Encapsula el consumo de UsuariosController/RolesController/PermisosController (backend,
// docs/05-api.md §55/§59-§63, ADR-061). Todos exigen JWT + SEGURIDAD_VER (lecturas) o
// SEGURIDAD_ADMINISTRAR (escrituras) desde el punto 8 — apiClient.ts adjunta el token
// automáticamente. Sin PUT/DELETE/activar-desactivar ni "quitar una asignación": ninguno existe
// en el backend (05-api.md §55.3/§63.3), no se inventan aquí.
export const seguridadService = {
  listarUsuarios: (): Promise<UsuarioAdmin[]> => apiClient.get<UsuarioAdmin[]>("/api/v1/usuarios"),

  crearUsuario: (request: CrearUsuarioRequest): Promise<UsuarioAdmin> =>
    apiClient.post<UsuarioAdmin>("/api/v1/usuarios", request),

  listarRolesDeUsuario: (usuarioId: number): Promise<Rol[]> =>
    apiClient.get<Rol[]>(`/api/v1/usuarios/${usuarioId}/roles`),

  asignarRolAUsuario: (usuarioId: number, rolId: number): Promise<{ usuarioId: number; rolId: number }> =>
    apiClient.post(`/api/v1/usuarios/${usuarioId}/roles`, { rolId }),

  listarRoles: (): Promise<Rol[]> => apiClient.get<Rol[]>("/api/v1/roles"),

  crearRol: (request: CrearRolRequest): Promise<Rol> => apiClient.post<Rol>("/api/v1/roles", request),

  listarPermisosDeRol: (rolId: number): Promise<Permiso[]> =>
    apiClient.get<Permiso[]>(`/api/v1/roles/${rolId}/permisos`),

  asignarPermisoARol: (rolId: number, permisoId: number): Promise<{ rolId: number; permisoId: number }> =>
    apiClient.post(`/api/v1/roles/${rolId}/permisos`, { permisoId }),

  listarPermisos: (): Promise<Permiso[]> => apiClient.get<Permiso[]>("/api/v1/permisos"),

  crearPermiso: (request: CrearPermisoRequest): Promise<Permiso> =>
    apiClient.post<Permiso>("/api/v1/permisos", request),
};
