import type { ApiRequestError } from "../services/apiClient";

// Banner de error reutilizable a partir de ApiRequestError (código + mensaje + detalles de
// campo). Usado por cada acción de la pantalla de Requisiciones que llama a la Api (crear,
// agregar/editar/eliminar detalle, guardar borrador) para no repetir el mismo marcado.
export function ErrorBanner({ error }: { error: ApiRequestError }) {
  return (
    <div className="rounded border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
      <p className="font-medium">
        {error.status || "Sin respuesta"} — {error.code}
      </p>
      <p>{error.message}</p>
      {error.details.length > 0 && (
        <ul className="mt-1 list-disc pl-5">
          {error.details.map((detalle) => (
            <li key={detalle}>{detalle}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
