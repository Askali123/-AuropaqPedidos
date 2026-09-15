using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-029/TASK-035, docs/05-api.md §23. Domain.Requisicion.Enviar() ya valida todas las
// reglas que le corresponden (detalles, empresa activa, ventana del periodo, distribución
// completa) y sirve tanto para el envío inicial (BORRADOR) como para el reenvío tras
// corrección (DEVUELTA) — esa decisión ya fue tomada en el bloque de Domain y no se repite aquí.
//
// TASK-055 (Logging, punto 8.1 — 2026-09-15): no se agrega un registro de Auditoria aparte —
// HistorialRequisicion ya cumple ese rol para este agregado (05-api.md §23). El log es
// puramente técnico/diagnóstico.
public sealed class EnviarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly ILogger<EnviarRequisicionUseCase> _logger;

    public EnviarRequisicionUseCase(IRequisicionRepository requisiciones, ILogger<EnviarRequisicionUseCase> logger)
    {
        _requisiciones = requisiciones;
        _logger = logger;
    }

    // usuarioId debe provenir de la identidad autenticada (docs/06-seguridad.md §17.3), no del cliente.
    public RequisicionResponse Ejecutar(int requisicionId, int usuarioId, DateTime fechaEnvio)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        requisicion.Enviar(usuarioId, fechaEnvio);

        _requisiciones.Guardar(requisicion);

        _logger.LogInformation("Requisición {RequisicionId} enviada por usuario {UsuarioId}", requisicionId, usuarioId);

        return RequisicionMapper.AResponse(requisicion);
    }
}
