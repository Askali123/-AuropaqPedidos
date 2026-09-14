using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-029/TASK-035, docs/05-api.md §23. Domain.Requisicion.Enviar() ya valida todas las
// reglas que le corresponden (detalles, empresa activa, ventana del periodo, distribución
// completa) y sirve tanto para el envío inicial (BORRADOR) como para el reenvío tras
// corrección (DEVUELTA) — esa decisión ya fue tomada en el bloque de Domain y no se repite aquí.
public sealed class EnviarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public EnviarRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.3), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fechaEnvio)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Enviar(usuarioId, fechaEnvio);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
