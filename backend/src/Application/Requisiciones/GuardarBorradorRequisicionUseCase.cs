using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-028 / RN-013: guardar no equivale a enviar. No hay una regla de Domain que "guardar"
// deba ejecutar (la requisición ya permanece en su estado hasta que se llame Enviar()); este
// caso de uso solo confirma que el estado actual quede persistido mediante la abstracción.
public sealed class GuardarBorradorRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public GuardarBorradorRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
