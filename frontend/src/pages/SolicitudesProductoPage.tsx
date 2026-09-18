import { Fragment, useState } from "react";
import { ErrorBanner } from "../components/ErrorBanner";
import { SolicitarProductoForm } from "../components/solicitudes/SolicitarProductoForm";
import { ResolverSolicitudPanel } from "../components/solicitudes/ResolverSolicitudPanel";
import { useAuth } from "../auth/AuthContext";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { solicitudesProductoService } from "../services/solicitudesProductoService";

// docs/05-api.md §28. Tres permisos distintos conviven en la misma pantalla
// (docs/06-seguridad.md §52): PRODUCTO_SOLICITAR (enviar una solicitud — Solicitante/
// Administrador), PRODUCTO_VER (ver la bandeja de pendientes — también Gestor_Requisiciones/
// Compras/Administrador) y PRODUCTO_CREAR (resolverlas: homologar/crear producto/rechazar —
// gestor de catálogo/Administrador). No es un simple "mitad Solicitante, mitad Gestor": un
// Solicitante SÍ tiene PRODUCTO_VER (puede ver la bandeja de pendientes, solo no resolverla).
//
// TASK-103 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F2): cada bloque de
// la pantalla ahora se muestra solo si el usuario tiene el permiso real correspondiente — antes
// se mostraban los tres bloques a cualquiera, y al Solicitante le fallaba "Resolver" con 403
// recién al hacer clic (el propio comentario de este archivo documentaba el defecto). El backend
// sigue siendo quien decide 401/403 en cada endpoint (03-arquitectura.md §997); esto es solo UX.
export function SolicitudesProductoPage() {
  const { tieneAlgunPermiso } = useAuth();
  const puedeSolicitar = tieneAlgunPermiso("PRODUCTO_SOLICITAR");
  const puedeVerPendientes = tieneAlgunPermiso("PRODUCTO_VER");
  const puedeResolver = tieneAlgunPermiso("PRODUCTO_CREAR");

  const [version, setVersion] = useState(0);
  const pendientes = useConsultaLista(
    () => (puedeVerPendientes ? solicitudesProductoService.listarPendientes() : Promise.resolve([])),
    [version, puedeVerPendientes],
  );

  const [expandidaId, setExpandidaId] = useState<number | null>(null);

  return (
    <main className="mx-auto max-w-3xl px-4 py-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Solicitudes de producto</h1>
          <p className="mt-1 text-sm text-slate-500">Productos que no existen en el catálogo.</p>
        </div>
        {puedeSolicitar && <SolicitarProductoForm onSolicitado={() => setVersion((v) => v + 1)} />}
      </div>

      {puedeVerPendientes && (
        <div className="mt-6 overflow-x-auto rounded-lg border border-slate-200 bg-white">
          {pendientes.cargando ? (
            <p className="p-4 text-sm text-slate-500">Cargando...</p>
          ) : pendientes.error ? (
            <div className="p-4">
              <ErrorBanner error={pendientes.error} />
            </div>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs text-slate-500">
                  <th className="px-4 py-2 font-medium">Id</th>
                  <th className="px-4 py-2 font-medium">Nombre solicitado</th>
                  <th className="px-4 py-2 font-medium">Descripción</th>
                  <th className="px-4 py-2 font-medium">Solicitada</th>
                  {puedeResolver && <th className="px-4 py-2 font-medium">Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {pendientes.datos.map((solicitud) => (
                  <Fragment key={solicitud.id}>
                    <tr className="border-b border-slate-100">
                      <td className="px-4 py-2 text-slate-900">{solicitud.id}</td>
                      <td className="px-4 py-2 text-slate-900">{solicitud.nombreSolicitado}</td>
                      <td className="px-4 py-2 text-slate-900">{solicitud.descripcion ?? "—"}</td>
                      <td className="px-4 py-2 text-slate-900">{solicitud.fechaSolicitud.slice(0, 10)}</td>
                      {puedeResolver && (
                        <td className="px-4 py-2">
                          <button
                            type="button"
                            onClick={() => setExpandidaId(expandidaId === solicitud.id ? null : solicitud.id)}
                            className="text-xs font-medium text-blue-600 underline hover:text-blue-800"
                          >
                            {expandidaId === solicitud.id ? "Ocultar" : "Resolver"}
                          </button>
                        </td>
                      )}
                    </tr>
                    {puedeResolver && expandidaId === solicitud.id && (
                      <tr>
                        <td colSpan={5} className="bg-slate-50 px-4 py-3">
                          <ResolverSolicitudPanel
                            solicitud={solicitud}
                            onResuelta={() => {
                              setExpandidaId(null);
                              setVersion((v) => v + 1);
                            }}
                          />
                        </td>
                      </tr>
                    )}
                  </Fragment>
                ))}
                {pendientes.datos.length === 0 && (
                  <tr>
                    <td colSpan={5} className="px-4 py-3 text-sm text-slate-500">
                      No hay solicitudes pendientes.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>
      )}
    </main>
  );
}
