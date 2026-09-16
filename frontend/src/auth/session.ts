import type { UsuarioAutenticado } from "../types/auth";

// Persistencia de sesión (token + usuario autenticado). Sin refresh tokens (fuera de alcance,
// docs/05-api.md §56 / TASK-048): cuando el JWT expira o el backend responde 401, la única
// acción posible es volver a iniciar sesión — por eso basta con localStorage, sin lógica de
// renovación. apiClient.ts lee el token de aquí para adjuntarlo a cada request; no importa
// React ni el AuthContext, para evitar un ciclo de dependencias.
const CLAVE_TOKEN = "auropaq.auth.token";
const CLAVE_USUARIO = "auropaq.auth.usuario";

type Escucha = () => void;
const escuchas = new Set<Escucha>();

function notificar() {
  escuchas.forEach((escucha) => escucha());
}

export function suscribirCambiosDeSesion(escucha: Escucha): () => void {
  escuchas.add(escucha);
  return () => escuchas.delete(escucha);
}

export function guardarSesion(token: string, usuario: UsuarioAutenticado): void {
  localStorage.setItem(CLAVE_TOKEN, token);
  localStorage.setItem(CLAVE_USUARIO, JSON.stringify(usuario));
  notificar();
}

export function limpiarSesion(): void {
  localStorage.removeItem(CLAVE_TOKEN);
  localStorage.removeItem(CLAVE_USUARIO);
  notificar();
}

export function obtenerToken(): string | null {
  return localStorage.getItem(CLAVE_TOKEN);
}

export function obtenerUsuario(): UsuarioAutenticado | null {
  const crudo = localStorage.getItem(CLAVE_USUARIO);
  if (!crudo) return null;
  try {
    return JSON.parse(crudo) as UsuarioAutenticado;
  } catch {
    return null;
  }
}
