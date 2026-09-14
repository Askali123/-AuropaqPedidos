import { useEffect, useState, type DependencyList } from "react";
import { ApiRequestError } from "../services/apiClient";

export interface EstadoConsultaLista<T> {
  datos: T[];
  cargando: boolean;
  error: ApiRequestError | null;
}

// Los cuatro selectores de la pantalla de Requisiciones (Empresa/Sede/Periodo/Producto)
// consultan un GET distinto pero necesitan exactamente los mismos tres estados de UI (cargando /
// error / datos, donde una lista vacía es un resultado válido, no un error). Este hook evita
// repetir ese mismo patrón cuatro veces. "deps" controla cuándo se vuelve a consultar (p. ej.
// [empresaId] para las sedes); "consultar" se re-ejecuta en cada cambio de deps.
export function useConsultaLista<T>(consultar: () => Promise<T[]>, deps: DependencyList): EstadoConsultaLista<T> {
  const [datos, setDatos] = useState<T[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<ApiRequestError | null>(null);

  useEffect(() => {
    let cancelado = false;
    setCargando(true);
    setError(null);

    consultar()
      .then((resultado) => {
        if (!cancelado) setDatos(resultado);
      })
      .catch((error: unknown) => {
        if (!cancelado) {
          setError(error instanceof ApiRequestError ? error : new ApiRequestError(0, "ERROR_DESCONOCIDO", String(error)));
          setDatos([]);
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false);
      });

    return () => {
      cancelado = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- "deps" lo controla quien llama.
  }, deps);

  return { datos, cargando, error };
}
