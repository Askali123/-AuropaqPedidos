using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-034 / docs/05-api.md §25. Domain.Requisicion.Devolver() ya exige EN_REVISION y motivo
// obligatorio (RN-018).
public sealed class DevolverRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public DevolverRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.5), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fecha, DevolverRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Devolver(usuarioId, fecha, request.Motivo);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
