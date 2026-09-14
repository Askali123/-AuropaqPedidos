using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

public sealed class ModificarDistribucionRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public ModificarDistribucionRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId, int detalleId, int distribucionId, int nuevaCantidad)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);
        var detalle = RequisicionFinder.ObtenerDetalleOLanzar(requisicion, detalleId);
        var distribucion = RequisicionFinder.ObtenerDistribucionOLanzar(detalle, distribucionId);

        requisicion.ModificarDistribucion(detalle, distribucion, nuevaCantidad);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
