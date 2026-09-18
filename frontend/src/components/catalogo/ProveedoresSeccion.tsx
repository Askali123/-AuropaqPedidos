import { Fragment, useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { ProductosDeProveedorPanel } from "./ProductosDeProveedorPanel";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import type { Proveedor } from "../../types/catalogos";

// docs/05-api.md §29. Mismo patrón que CategoriasSeccion/UnidadesMedidaSeccion: lista + alta +
// edición en línea. Sin DELETE: no existe en el backend. "Productos de este proveedor" (§30,
// ListarProductos, TASK-019) se agregó como panel expandible de solo lectura
// (ProductosDeProveedorPanel) — la asociación en sí se crea desde ProductosSeccion, no aquí.
export function ProveedoresSeccion() {
  const [version, setVersion] = useState(0);
  const proveedores = useConsultaLista(() => catalogosService.listarProveedores(), [version]);

  const [formAbierto, setFormAbierto] = useState(false);
  const [nombre, setNombre] = useState("");
  const [nit, setNit] = useState("");
  const [contacto, setContacto] = useState("");
  const [telefono, setTelefono] = useState("");
  const [correo, setCorreo] = useState("");
  const [creando, setCreando] = useState(false);
  const [errorCrear, setErrorCrear] = useState<ApiRequestError | null>(null);

  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [expandidoId, setExpandidoId] = useState<number | null>(null);

  async function crearProveedor(evento: React.FormEvent) {
    evento.preventDefault();
    setCreando(true);
    setErrorCrear(null);
    try {
      await catalogosService.crearProveedor({
        nombre,
        nit: nit || undefined,
        contacto: contacto || undefined,
        telefono: telefono || undefined,
        correo: correo || undefined,
      });
      setVersion((v) => v + 1);
      setFormAbierto(false);
      setNombre("");
      setNit("");
      setContacto("");
      setTelefono("");
      setCorreo("");
    } catch (error) {
      setErrorCrear(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
    } finally {
      setCreando(false);
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-slate-800">Proveedores</h2>
        <button
          type="button"
          onClick={() => setFormAbierto((abierto) => !abierto)}
          className="rounded bg-blue-600 px-3 py-1.5 text-sm font-medium text-white"
        >
          {formAbierto ? "Cancelar" : "Nuevo proveedor"}
        </button>
      </div>

      {formAbierto && (
        <form onSubmit={crearProveedor} className="mt-3 rounded border border-slate-200 bg-slate-50 p-3">
          <div className="grid gap-3 sm:grid-cols-2">
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
              <label className="text-sm font-medium text-slate-700">Nit (opcional)</label>
              <input
                value={nit}
                onChange={(evento) => setNit(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Contacto (opcional)</label>
              <input
                value={contacto}
                onChange={(evento) => setContacto(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Teléfono (opcional)</label>
              <input
                value={telefono}
                onChange={(evento) => setTelefono(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Correo (opcional)</label>
              <input
                type="email"
                value={correo}
                onChange={(evento) => setCorreo(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={creando}
            className="mt-3 rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {creando ? "Creando..." : "Crear proveedor"}
          </button>

          {errorCrear && (
            <div className="mt-2">
              <ErrorBanner error={errorCrear} />
            </div>
          )}
        </form>
      )}

      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white">
        {proveedores.cargando ? (
          <p className="p-4 text-sm text-slate-500">Cargando...</p>
        ) : proveedores.error ? (
          <div className="p-4">
            <ErrorBanner error={proveedores.error} />
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs text-slate-500">
                <th className="px-4 py-2 font-medium">Id</th>
                <th className="px-4 py-2 font-medium">Nombre</th>
                <th className="px-4 py-2 font-medium">Nit</th>
                <th className="px-4 py-2 font-medium">Contacto</th>
                <th className="px-4 py-2 font-medium">Teléfono</th>
                <th className="px-4 py-2 font-medium">Correo</th>
                <th className="px-4 py-2 font-medium">Activo</th>
                <th className="px-4 py-2 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {proveedores.datos.map((proveedor) => (
                <Fragment key={proveedor.id}>
                  {editandoId === proveedor.id ? (
                    <FilaEdicion
                      proveedor={proveedor}
                      onCancelar={() => setEditandoId(null)}
                      onGuardado={() => {
                        setEditandoId(null);
                        setVersion((v) => v + 1);
                      }}
                    />
                  ) : (
                    <tr className="border-b border-slate-100">
                      <td className="px-4 py-2 text-slate-900">{proveedor.id}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.nombre}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.nit ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.contacto ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.telefono ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.correo ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{proveedor.activo ? "Sí" : "No"}</td>
                      <td className="px-4 py-2">
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            onClick={() => setEditandoId(proveedor.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            Editar
                          </button>
                          <button
                            type="button"
                            onClick={() => setExpandidoId(expandidoId === proveedor.id ? null : proveedor.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            {expandidoId === proveedor.id ? "Ocultar productos" : "Ver productos"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                  {expandidoId === proveedor.id && (
                    <tr>
                      <td colSpan={8} className="bg-slate-50 px-4 py-3">
                        <ProductosDeProveedorPanel proveedorId={proveedor.id} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {proveedores.datos.length === 0 && (
                <tr>
                  <td colSpan={8} className="px-4 py-3 text-sm text-slate-500">
                    No hay proveedores registrados.
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

function FilaEdicion({
  proveedor,
  onCancelar,
  onGuardado,
}: {
  proveedor: Proveedor;
  onCancelar: () => void;
  onGuardado: () => void;
}) {
  const [nombre, setNombre] = useState(proveedor.nombre);
  const [nit, setNit] = useState(proveedor.nit ?? "");
  const [contacto, setContacto] = useState(proveedor.contacto ?? "");
  const [telefono, setTelefono] = useState(proveedor.telefono ?? "");
  const [correo, setCorreo] = useState(proveedor.correo ?? "");
  const [activo, setActivo] = useState(proveedor.activo);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function guardar() {
    setGuardando(true);
    setError(null);
    try {
      await catalogosService.actualizarProveedor(proveedor.id, {
        nombre,
        nit: nit || null,
        contacto: contacto || null,
        telefono: telefono || null,
        correo: correo || null,
        activo,
      });
      onGuardado();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setGuardando(false);
    }
  }

  return (
    <tr className="border-b border-slate-100 bg-slate-50">
      <td className="px-4 py-2 text-slate-900">{proveedor.id}</td>
      <td className="px-4 py-2">
        <input
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          value={nit}
          onChange={(evento) => setNit(evento.target.value)}
          className="w-24 rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          value={contacto}
          onChange={(evento) => setContacto(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          value={telefono}
          onChange={(evento) => setTelefono(evento.target.value)}
          className="w-28 rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input
          type="email"
          value={correo}
          onChange={(evento) => setCorreo(evento.target.value)}
          className="w-full rounded border border-slate-300 bg-white px-2 py-1 text-sm text-slate-900"
        />
      </td>
      <td className="px-4 py-2">
        <input type="checkbox" checked={activo} onChange={(evento) => setActivo(evento.target.checked)} />
      </td>
      <td className="px-4 py-2">
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={guardar}
            disabled={guardando}
            className="rounded bg-blue-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
          >
            {guardando ? "Guardando..." : "Guardar"}
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
