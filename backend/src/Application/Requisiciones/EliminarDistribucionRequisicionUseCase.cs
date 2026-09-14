using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

public sealed class EliminarDistribucionRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public EliminarDistribucionRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId, int detalleId, int distribucionId)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);
        var detalle = RequisicionFinder.ObtenerDetalleOLanzar(requisicion, detalleId);
        var distribucion = RequisicionFinder.ObtenerDistribucionOLanzar(detalle, distribucionId);

        requisicion.EliminarDistribucion(detalle, distribucion);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
