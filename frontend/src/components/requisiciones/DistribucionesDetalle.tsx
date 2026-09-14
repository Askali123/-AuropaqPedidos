import { DistribucionRequisicionRow } from "./DistribucionRequisicionRow";
import { AgregarDistribucionForm } from "./AgregarDistribucionForm";
import type { ApiRequestError } from "../../services/apiClient";
import type { Sede } from "../../types/catalogos";
import type { DetalleRequisicion, Requisicion } from "../../types/requisicion";

interface DistribucionesDetalleProps {
  requisicionId: number;
  detalle: DetalleRequisicion;
  sedes: Sede[];
  sedesCargando: boolean;
  sedesError: ApiRequestError | null;
  editable: boolean;
  onActualizado: (requisicion: Requisicion) => void;
}

function nombreSede(sedes: Sede[], sedeId: number): string {
  return sedes.find((sede) => sede.id === sedeId)?.nombre ?? `Sede ${sedeId}`;
}

// Tercera etapa del Frontend de Requisiciones: distribución por sede de UN detalle
// (docs/05-api.md §54.4, filas 5-7). Se apoya en las Sedes que ya consulta RequisicionesPage
// (catalogosService.listarSedesPorEmpresa, §54.6.1) — no vuelve a pedirlas ni inventa un
// catálogo propio. cantidadDistribuida/distribucionCompleta llegan ya calculados desde la Api
// (DetalleRequisicionResponse), no se recalculan aquí.
export function DistribucionesDetalle({
  requisicionId,
  detalle,
  sedes,
  sedesCargando,
  sedesError,
  editable,
  onActualizado,
}: DistribucionesDetalleProps) {
  const faltante = detalle.cantidadSolicitada - detalle.cantidadDistribuida;

  return (
    <div className="rounded border border-slate-200 bg-slate-50 px-3 py-2">
      <p className="text-xs text-slate-500">
        Distribuido: <strong className="text-slate-700">{detalle.cantidadDistribuida}</strong> / {detalle.cantidadSolicitada}
        {" — "}
        {detalle.distribucionCompleta ? (
          <span className="font-medium text-green-700">completa</span>
        ) : (
          <span className="font-medium text-amber-700">falta {faltante}</span>
        )}
      </p>

      {detalle.distribuciones.length > 0 ? (
        <ul className="mt-2">
          {detalle.distribuciones.map((distribucion) => (
            <DistribucionRequisicionRow
              key={distribucion.id}
              requisicionId={requisicionId}
              detalleId={detalle.id}
              distribucion={distribucion}
              nombreSede={nombreSede(sedes, distribucion.sedeId)}
              maximoDisponibleParaModificar={detalle.cantidadSolicitada - (detalle.cantidadDistribuida - distribucion.cantidad)}
              editable={editable}
              onActualizado={onActualizado}
            />
          ))}
        </ul>
      ) : (
        <p className="mt-2 text-xs text-slate-400 italic">Todavía no se ha distribuido este producto entre sedes.</p>
      )}

      {editable && (
        <div className="mt-2 border-t border-slate-200 pt-2">
          <AgregarDistribucionForm
            requisicionId={requisicionId}
            detalleId={detalle.id}
            sedes={sedes}
            sedesCargando={sedesCargando}
            sedesError={sedesError}
            cantidadDisponible={faltante}
            onAgregado={onActualizado}
          />
        </div>
      )}
    </div>
  );
}
