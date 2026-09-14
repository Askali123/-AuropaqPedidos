using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-033 / docs/05-api.md §24.2. Domain.Requisicion.Aprobar() ya exige EN_REVISION (RN-017).
public sealed class AprobarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public AprobarRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.4), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fecha, AprobarRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Aprobar(usuarioId, fecha, request.Observacion);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
