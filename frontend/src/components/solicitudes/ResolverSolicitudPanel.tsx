import { useState } from "react";
import { ErrorBanner } from "../ErrorBanner";
import { SelectorCatalogo } from "../requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../../hooks/useConsultaLista";
import { ApiRequestError } from "../../services/apiClient";
import { catalogosService } from "../../services/catalogosService";
import { solicitudesProductoService } from "../../services/solicitudesProductoService";
import type { Categoria, Producto, UnidadMedida } from "../../types/catalogos";
import type { SolicitudProductoCatalogo } from "../../types/solicitudesProducto";

interface ResolverSolicitudPanelProps {
  solicitud: SolicitudProductoCatalogo;
  onResuelta: () => void;
}

type Accion = "homologar" | "crear" | "rechazar";

// docs/05-api.md §28.3-§28.5: las tres resoluciones posibles de una solicitud pendiente
// (02-dominio.md, diagrama Homologar/Crear producto/Rechazar). Se muestran como pestañas dentro
// del mismo panel expandible por fila en vez de tres pantallas separadas — son mutuamente
// excluyentes sobre la misma solicitud.
export function ResolverSolicitudPanel({ solicitud, onResuelta }: ResolverSolicitudPanelProps) {
  const [accion, setAccion] = useState<Accion>("homologar");

  const productos = useConsultaLista<Producto>(() => catalogosService.listarProductos(), []);
  const categorias = useConsultaLista<Categoria>(() => catalogosService.listarCategorias(), []);
  const unidades = useConsultaLista<UnidadMedida>(() => catalogosService.listarUnidadesMedida(), []);

  const [productoId, setProductoId] = useState<number | null>(null);
  const [categoriaId, setCategoriaId] = useState<number | null>(null);
  const [unidadMedidaId, setUnidadMedidaId] = useState<number | null>(null);
  const [codigoInterno, setCodigoInterno] = useState("");
  const [motivo, setMotivo] = useState("");

  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<ApiRequestError | null>(null);

  async function homologar() {
    if (productoId == null) return;
    setEnviando(true);
    setError(null);
    try {
      await solicitudesProductoService.homologar(solicitud.id, { productoId });
      onResuelta();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setEnviando(false);
    }
  }

  async function crearProducto() {
    if (categoriaId == null || unidadMedidaId == null) return;
    setEnviando(true);
    setError(null);
    try {
      await solicitudesProductoService.crearProducto(solicitud.id, {
        categoriaId,
        unidadMedidaId,
        codigoInterno: codigoInterno || undefined,
      });
      onResuelta();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setEnviando(false);
    }
  }

  async function rechazar() {
    if (motivo.trim() === "") return;
    setEnviando(true);
    setError(null);
    try {
      await solicitudesProductoService.rechazar(solicitud.id, { motivo });
      onResuelta();
    } catch (error) {
      setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
      setEnviando(false);
    }
  }

  return (
    <div className="mt-2 rounded border border-slate-200 bg-slate-50 p-3">
      <div className="flex gap-2 border-b border-slate-200 pb-2">
        <PestanaAccion actual={accion} valor="homologar" onSeleccionar={setAccion}>
          Homologar
        </PestanaAccion>
        <PestanaAccion actual={accion} valor="crear" onSeleccionar={setAccion}>
          Crear producto
        </PestanaAccion>
        <PestanaAccion actual={accion} valor="rechazar" onSeleccionar={setAccion}>
          Rechazar
        </PestanaAccion>
      </div>

      {accion === "homologar" && (
        <div className="mt-3">
          <p className="text-xs text-slate-500">Vincula esta solicitud a un producto que ya existe en el catálogo.</p>
          <div className="mt-2 max-w-sm">
            <SelectorCatalogo
              etiqueta="Producto existente"
              datos={productos.datos}
              cargando={productos.cargando}
              error={productos.error}
              valorSeleccionado={productoId}
              onSeleccionar={setProductoId}
              obtenerId={(producto) => producto.id}
              obtenerTexto={(producto) => producto.nombre}
              mensajeVacio="No hay productos registrados."
            />
          </div>
          <button
            type="button"
            onClick={homologar}
            disabled={productoId == null || enviando}
            className="mt-3 rounded bg-blue-600 px-3 py-1.5 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {enviando ? "Homologando..." : "Homologar"}
          </button>
        </div>
      )}

      {accion === "crear" && (
        <div className="mt-3">
          <p className="text-xs text-slate-500">
            Crea un producto nuevo en el catálogo a partir de esta solicitud ("{solicitud.nombreSolicitado}").
          </p>
          <div className="mt-2 grid gap-2 sm:grid-cols-3">
            <SelectorCatalogo
              etiqueta="Categoría"
              datos={categorias.datos}
              cargando={categorias.cargando}
              error={categorias.error}
              valorSeleccionado={categoriaId}
              onSeleccionar={setCategoriaId}
              obtenerId={(categoria) => categoria.id}
              obtenerTexto={(categoria) => categoria.nombre}
              mensajeVacio="No hay categorías registradas."
            />
            <SelectorCatalogo
              etiqueta="Unidad de medida"
              datos={unidades.datos}
              cargando={unidades.cargando}
              error={unidades.error}
              valorSeleccionado={unidadMedidaId}
              onSeleccionar={setUnidadMedidaId}
              obtenerId={(unidad) => unidad.id}
              obtenerTexto={(unidad) => `${unidad.codigo} — ${unidad.nombre}`}
              mensajeVacio="No hay unidades de medida registradas."
            />
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">Código interno (opcional)</label>
              <input
                value={codigoInterno}
                onChange={(evento) => setCodigoInterno(evento.target.value)}
                className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
              />
            </div>
          </div>
          <button
            type="button"
            onClick={crearProducto}
            disabled={categoriaId == null || unidadMedidaId == null || enviando}
            className="mt-3 rounded bg-blue-600 px-3 py-1.5 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {enviando ? "Creando..." : "Crear producto"}
          </button>
        </div>
      )}

      {accion === "rechazar" && (
        <div className="mt-3">
          <label className="text-sm font-medium text-slate-700">Motivo (obligatorio)</label>
          <input
            value={motivo}
            onChange={(evento) => setMotivo(evento.target.value)}
            className="mt-1 w-full max-w-sm rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900"
          />
          <button
            type="button"
            onClick={rechazar}
            disabled={motivo.trim() === "" || enviando}
            className="mt-3 block rounded bg-amber-600 px-3 py-1.5 text-xs font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            {enviando ? "Rechazando..." : "Rechazar"}
          </button>
        </div>
      )}

      {error && (
        <div className="mt-3">
          <ErrorBanner error={error} />
        </div>
      )}
    </div>
  );
}

function PestanaAccion({
  actual,
  valor,
  onSeleccionar,
  children,
}: {
  actual: Accion;
  valor: Accion;
  onSeleccionar: (accion: Accion) => void;
  children: string;
}) {
  const activo = actual === valor;
  return (
    <button
      type="button"
      onClick={() => onSeleccionar(valor)}
      className={`px-2 py-1 text-xs font-medium ${activo ? "text-blue-600" : "text-slate-500 hover:text-slate-700"}`}
    >
      {children}
    </button>
  );
}
