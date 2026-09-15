using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-034 / docs/05-api.md §25. Domain.Requisicion.Devolver() ya exige EN_REVISION y motivo
// obligatorio (RN-018).
// TASK-055 (Logging, punto 8.1): ver nota de EnviarRequisicionUseCase sobre por qué no se
// duplica con un registro de Auditoria (HistorialRequisicion ya cumple ese rol).
public sealed class DevolverRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly ILogger<DevolverRequisicionUseCase> _logger;

    public DevolverRequisicionUseCase(IRequisicionRepository requisiciones, ILogger<DevolverRequisicionUseCase> logger)
    {
        _requisiciones = requisiciones;
        _logger = logger;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.5), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fecha, DevolverRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Devolver(usuarioId, fecha, request.Motivo);

        _requisiciones.Guardar(requisicion);

        _logger.LogInformation("Requisición {RequisicionId} devuelta por usuario {UsuarioId}", requisicionId, usuarioId);

        return RequisicionMapper.AResponse(requisicion);
    }
}
