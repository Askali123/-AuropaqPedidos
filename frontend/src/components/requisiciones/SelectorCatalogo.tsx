import type { ApiRequestError } from "../../services/apiClient";

interface SelectorCatalogoProps<T> {
  etiqueta: string;
  datos: T[];
  cargando: boolean;
  error: ApiRequestError | null;
  valorSeleccionado: number | null;
  onSeleccionar: (id: number | null) => void;
  obtenerId: (item: T) => number;
  obtenerTexto: (item: T) => string;
  deshabilitado?: boolean;
  mensajeDeshabilitado?: string;
  mensajeVacio?: string;
}

// Un único componente para los cuatro selectores de la pantalla de Requisiciones
// (Empresa/Sede/Periodo/Producto): misma estructura visual y los mismos cuatro estados
// (deshabilitado / cargando / error / datos-o-lista-vacía) para los cuatro, en vez de repetir el
// mismo JSX cuatro veces con etiquetas distintas.
export function SelectorCatalogo<T>({
  etiqueta,
  datos,
  cargando,
  error,
  valorSeleccionado,
  onSeleccionar,
  obtenerId,
  obtenerTexto,
  deshabilitado = false,
  mensajeDeshabilitado,
  mensajeVacio = "No hay opciones disponibles.",
}: SelectorCatalogoProps<T>) {
  return (
    <div className="flex flex-col gap-1">
      <label className="text-sm font-medium text-slate-700">{etiqueta}</label>

      {deshabilitado ? (
        <p className="text-sm text-slate-400 italic">{mensajeDeshabilitado ?? "No disponible todavía."}</p>
      ) : cargando ? (
        <p className="text-sm text-slate-500">Cargando...</p>
      ) : error ? (
        <div className="rounded border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
          <p className="font-medium">{error.code}</p>
          <p>{error.message}</p>
        </div>
      ) : datos.length === 0 ? (
        <p className="text-sm text-slate-500">{mensajeVacio}</p>
      ) : (
        <select
          className="rounded border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 focus:border-blue-500 focus:outline-none"
          value={valorSeleccionado ?? ""}
          onChange={(evento) => onSeleccionar(evento.target.value ? Number(evento.target.value) : null)}
        >
          <option value="">Seleccionar...</option>
          {datos.map((item) => (
            <option key={obtenerId(item)} value={obtenerId(item)}>
              {obtenerTexto(item)}
            </option>
          ))}
        </select>
      )}
    </div>
  );
}
