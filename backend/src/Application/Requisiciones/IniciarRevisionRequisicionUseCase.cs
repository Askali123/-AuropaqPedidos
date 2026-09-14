using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// Transición ENVIADA -> EN_REVISION, ya implementada en Domain como Requisicion.IniciarRevision()
// (decisión que se mantiene sin cambios, según instrucción explícita del usuario).
public sealed class IniciarRevisionRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public IniciarRevisionRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fecha)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.IniciarRevision(usuarioId, fecha);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
