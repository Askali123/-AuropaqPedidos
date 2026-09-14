using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-024 / docs/05-api.md §20: un único caso de uso para cantidad y/o observación,
// igual que el PUT único documentado. Un campo en null significa "no modificarlo".
public sealed class ActualizarDetalleRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public ActualizarDetalleRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId, int detalleId, ActualizarDetalleRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);
        var detalle = RequisicionFinder.ObtenerDetalleOLanzar(requisicion, detalleId);

        if (request.CantidadSolicitada is not null)
            requisicion.ModificarCantidadDetalle(detalle, request.CantidadSolicitada.Value);

        if (request.Observacion is not null)
            requisicion.ModificarObservacionDetalle(detalle, request.Observacion);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
