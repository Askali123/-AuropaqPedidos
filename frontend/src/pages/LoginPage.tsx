import { useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

// docs/05-api.md §56.1. Correo/password inválidos y usuario inactivo responden todos el mismo
// 401 genérico (CREDENCIALES_INVALIDAS) — el formulario no distingue esos casos, solo muestra el
// mensaje que llega de la Api.
export function LoginPage() {
  const { usuario, iniciandoSesion, errorLogin, login } = useAuth();
  const location = useLocation();
  const [correo, setCorreo] = useState("");
  const [password, setPassword] = useState("");

  if (usuario) {
    const destino = (location.state as { desde?: string } | null)?.desde ?? "/requisiciones";
    return <Navigate to={destino} replace />;
  }

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault();
    try {
      await login(correo, password);
    } catch {
      // El error ya queda en errorLogin (AuthContext); nada más que hacer aquí.
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-100 px-4">
      <form onSubmit={enviar} className="w-full max-w-sm rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-xl font-semibold text-slate-900">AuropaqPedidos</h1>
        <p className="mt-1 text-sm text-slate-500">Inicia sesión para continuar.</p>

        <label className="mt-5 block text-sm font-medium text-slate-700">
          Correo
          <input
            type="email"
            value={correo}
            onChange={(evento) => setCorreo(evento.target.value)}
            required
            autoFocus
            className="mt-1 w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
          />
        </label>

        <label className="mt-4 block text-sm font-medium text-slate-700">
          Contraseña
          <input
            type="password"
            value={password}
            onChange={(evento) => setPassword(evento.target.value)}
            required
            minLength={8}
            className="mt-1 w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
          />
        </label>

        <button
          type="submit"
          disabled={iniciandoSesion}
          className="mt-5 w-full rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {iniciandoSesion ? "Ingresando..." : "Ingresar"}
        </button>

        {errorLogin && (
          <p className="mt-3 text-sm text-red-700" role="alert">
            {errorLogin.message}
          </p>
        )}
      </form>
    </main>
  );
}
