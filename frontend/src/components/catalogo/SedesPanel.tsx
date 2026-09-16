import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Sede } from "../../types/catalogos";

interface SedesPanelProps {
  empresaId: number;
}

// docs/05-api.md §12. Panel expandible por fila de EmpresasSeccion (mismo patrón que
// UsuarioRolesPanel/RolPermisosPanel en Seguridad): lista las sedes de esa Empresa (§12.1,
// anidado bajo /empresas/{id}/sedes), permite crear una nueva (§12.2, misma ruta) y editar en
// línea (§12.3, PUT /sedes/{id} — ruta propia, no anidada). Sin DELETE: no existe en el backend.
export function SedesPanel({ empresaId }: SedesPanelProps) {
  const [version, setVersion] = useState(0);
  const sedes = useConsultaLista<Sede>(() => catalogosService.listarSedesPorEmpresa(empresaId), [empresaId, version]);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [direccion, setDireccion] = useState("");
  const [ciudad, setCiudad] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);

  async function crearSede(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.crearSede(empresaId, {
        nombre,
        direccion: direccion || undefined,
        ciudad: ciudad || undefined,
      });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setNombre("");
      setDireccion("");
      setCiudad("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <div className="flex items-center justify-between">
        <h4 className="text-xs font-semibold text-slate-600">Sedes</h4>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
        >
          {formAbierto ? "Cancelar" : "Nueva sede"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearSede} className="mt-2 grid gap-2 sm:grid-cols-3">
          <input
            required
            placeholder="Nombre"
            value={nombre}
            onChange={(evento) => setNombre(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
          <input
            placeholder="Dirección (opcional)"
            value={direccion}
            onChange={(evento) => setDireccion(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
          <input
            placeholder="Ciudad (opcional)"
            value={ciudad}
            onChange={(evento) => setCiudad(evento.target.value)}
            className="rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
          />
          <button
            type="submit"
            disabled={creando}
            className="rounded bg-slate-700 px-3 py-1 text-xs font-medium text-white disabled:opacity-50 sm:col-span-3 sm:w-fit"
          >
            {creando ? "Creando..." : "Crear sede"}
          </button>
        </form>
      )}

      {errorCrear && (
        <div className="mt-2">
          <ErrorBanner error={errorCrear} />
        </div>
      )}

      <div className="mt-3">
        {sedes.cargando ? (
          <p className="text-sm text-slate-500">Cargando...</p>
        ) : sedes.error ? (
          <ErrorBanner error={sedes.error} />
        ) : sedes.datos.length === 0 ? (
          <p className="text-sm text-slate-500">Sin sedes registradas.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="py-1 pr-3 font-medium">Nombre</th>
                <th className="py-1 pr-3 font-medium">Ciudad</th>
                <th className="py-1 pr-3 font-medium">Teléfono</th>
                <th className="py-1 pr-3 font-medium">Activo</th>
                <th className="py-1 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {sedes.datos.map((sede) =>
                editandoId === sede.id ? (
                  <FilaEdicion
                    key={sede.id}
                    sede={sede}
                    onCancelar={() => setEditandoId(null)}
                    onGuardado={() => {
                      setEditandoId(null);
                      setVersion((v) => v + 1);
                    }}
                  />
                ) : (
                  <tr key={sede.id} className="border-b border-slate-100">
                    <td className="py-1 pr-3 text-slate-900">{sede.nombre}</td>
                    <td className="py-1 pr-3 text-slate-900">{sede.ciudad ?? "—"}</td>
                    <td className="py-1 pr-3 text-slate-900">{sede.telefono ?? "—"}</td>
                    <td className="py-1 pr-3 text-slate-900">{sede.activo ? "Sí" : "No"}</td>
                    <td className="py-1">
                      <button
                        type="button"
                        onClick={() => setEditandoId(sede.id)}
                        className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                      >
                        Editar
                      </button>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

function FilaEdicion({ sede, onCancelar, onGuardado }: { sede: Sede; onCancelar: () => void; onGuardado: () => void }) {
  const [nombre, setNombre] = useState(sede.nombre);
  const [ciudad, setCiudad] = useState(sede.ciudad ?? "");
  const [telefono, setTelefono] = useState(sede.telefono ?? "");
  const [activo, setActivo] = useState(sede.activo);
  // Dirección/Departamento/Contacto no tienen input en esta fila compacta (sin espacio en la
  // tabla) — se reenvían tal cual venían para no perderlos en el PUT (que reemplaza todo el
  // objeto, ActualizarSedeRequest).
  const direccion = sede.direccion ?? "";
  const departamento = sede.departamento ?? "";
  const contacto = sede.contacto ?? "";
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarSede(sede.id, {
        nombre,
        direccion: direccion || null,
        ciudad: ciudad || null,
        departamento: departamento || null,
        telefono: telefono || null,
        contacto: contacto || null,
        activo,
      });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100">
      <td className="py-1 pr-3">
        <input
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="py-1 pr-3">
        <input
          value={ciudad}
          onChange={(evento) => setCiudad(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="py-1 pr-3">
        <input
          value={telefono}
          onChange={(evento) => setTelefono(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="py-1 pr-3">
        <input type="checkbox" checked={activo} onChange={(evento) => setActivo(evento.target.checked)} />
      </td>
      <td className="py-1">
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={guardar}
            disabled={guardando}
            className="rounded bg-blue-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
          >
            {guardando ? "..." : "Guardar"}
          </button>
          <button type="button" onClick={onCancelar} disabled={guardando} className="text-xs font-medium text-slate-500">
            Cancelar
          </button>
        </div>
        {error && (
          <div className="mt-1">
            <ErrorBanner error={error} />
          </div>
        )}
      </td>
    </tr>
  );
}
