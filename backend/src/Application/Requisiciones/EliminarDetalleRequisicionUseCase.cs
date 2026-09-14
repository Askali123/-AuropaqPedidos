using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-025 / docs/05-api.md §21.
public sealed class EliminarDetalleRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public EliminarDetalleRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId, int detalleId)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);
        var detalle = RequisicionFinder.ObtenerDetalleOLanzar(requisicion, detalleId);

        requisicion.EliminarDetalle(detalle);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
