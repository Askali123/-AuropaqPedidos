using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-033 / docs/05-api.md §24.2. Domain.Requisicion.Aprobar() ya exige EN_REVISION (RN-017).
// TASK-055 (Logging, punto 8.1): ver nota de EnviarRequisicionUseCase sobre por qué no se
// duplica con un registro de Auditoria (HistorialRequisicion ya cumple ese rol).
public sealed class AprobarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly ILogger<AprobarRequisicionUseCase> _logger;

    public AprobarRequisicionUseCase(IRequisicionRepository requisiciones, ILogger<AprobarRequisicionUseCase> logger)
    {
        _requisiciones = requisiciones;
        _logger = logger;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.4), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fecha, AprobarRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Aprobar(usuarioId, fecha, request.Observacion);

        _requisiciones.Guardar(requisicion);

        _logger.LogInformation("Requisición {RequisicionId} aprobada por usuario {UsuarioId}", requisicionId, usuarioId);

        return RequisicionMapper.AResponse(requisicion);
    }
}
