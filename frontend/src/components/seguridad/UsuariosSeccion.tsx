import { Fragment, useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorCatalogo } from "../requisiciones/SelectorCatalogo";
import { UsuarioRolesPanel } from "./UsuarioRolesPanel";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import { seguridadService } from "../../services/seguridadService";
import type { Empresa } from "../../types/catalogos";
import type { Rol } from "../../types/seguridad";

// docs/05-api.md §55/§61. Lista de Usuarios (§55.1) + alta (§55.2, exige EmpresaId+Password) +
// panel expandible por fila con sus roles (§61). Sin GET /usuarios/{id}, sin PUT/DELETE/
// activar-desactivar: ninguno existe en el backend (§55.3), no se inventan aquí.
export function UsuariosSeccion() {
  const [version, setVersion] = useState(0);
  const usuarios = useConsultaLista(() => seguridadService.listarUsuarios(), [version]);
  const empresas = useConsultaLista<Empresa>(() => catalogosService.listarEmpresas(), []);
  const roles = useConsultaLista<Rol>(() => seguridadService.listarRoles(), []);

  const [expandidoId, setExpandidoId] = useState<number | null>(null);

  const [formAbierto, setFormAbierto] = useState(false);
  const [empresaId, setEmpresaId] = useState<number | null>(null);
  const [nombre, setNombre] = useState("");
  const [apellido, setApellido] = useState("");
  const [correo, setCorreo] = useState("");
  const [password, setPassword] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  async function crearUsuario(evento: React.FormEvent) {
    evento.preventDefault();
    if (empresaId == null) return;

    setCreando(true);
    setErrorCrear(null);
    try {
      await seguridadService.crearUsuario({
        empresaId,
        nombre,
        correo,
        password,
        apellido: apellido || undefined,
      });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setEmpresaId(null);
      setNombre("");
      setApellido("");
      setCorreo("");
      setPassword("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-slate-800">Usuarios</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nuevo usuario"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearUsuario} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <SelectorCatalogo
              etiqueta="Empresa"
              datos={empresas.datos}
              cargando={empresas.cargando}
              error={empresas.error}
              valorSeleccionado={empresaId}
              onSeleccionar={setEmpresaId}
              obtenerId={(empresa) => empresa.id}
              obtenerTexto={(empresa) => empresa.nombre}
              mensajeVacio="No hay empresas registradas."
            />
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Nombre</label>
              <input
                required
                value={nombre}
                onChange={(evento) => setNombre(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Apellido (opcional)</label>
              <input
                value={apellido}
                onChange={(evento) => setApellido(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Correo</label>
              <input
                type="email"
                required
                value={correo}
                onChange={(evento) => setCorreo(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Contraseña</label>
              <input
                type="password"
                required
                minLength={8}
                value={password}
                onChange={(evento) => setPassword(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={empresaId == null || creando}
            className="mt-3 rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Creando..." : "Crear usuario"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {usuarios.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : usuarios.error ? (
          <div className="p-4">
            <ErrorBanner error={usuarios.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Correo</th>
                <th className="px-4 py-2 font-medium">Empresa</th>
                <th className="px-4 py-2 font-medium">Activo</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {usuarios.datos.map((usuario) => (
                <Fragment key={usuario.id}>
                  <tr className="border-b border-slate-100">
                    <td className="px-4 py-2 text-slate-900">{usuario.id}</td>
                    <td className="px-4 py-2 text-slate-900">
                      {usuario.nombre} {usuario.apellido ?? ""}
                    </td>
                    <td className="px-4 py-2 text-slate-900">{usuario.correo}</td>
                    <td className="px-4 py-2 text-slate-900">
                      {empresas.datos.find((empresa) => empresa.id === usuario.empresaId)?.nombre ?? usuario.empresaId}
                    </td>
                    <td className="px-4 py-2 text-slate-900">{usuario.activo ? "Sí" : "No"}</td>
                    <td className="px-4 py-2">
                      <button
                        type="button"
                        onClick={() => setExpandidoId(expandidoId === usuario.id ? null : usuario.id)}
                        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                      >
                        {expandidoId === usuario.id ? "Ocultar roles" : "Ver roles"}
                      </button>
                    </td>
                  </tr>
                  {expandidoId === usuario.id && (
                    <tr>
                      <td colSpan={6} className="bg-slate-50 px-4 py-3">
                        <UsuarioRolesPanel usuarioId={usuario.id} todosLosRoles={roles.datos} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {usuarios.datos.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-3 text-sm text-slate-500">
                    No hay usuarios registrados.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
