using AuropaqPedidos.Application.Auditorias.Abstracciones;
using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Entregas;

// TASK-042, RN-033: registra una entrega (posiblemente parcial) asociada a un PedidoProveedor
// existente. NumeroRemision se recibe explícito del llamador (mismo criterio que
// PedidoProveedor.NumeroPedido).
//
// TASK-056 (Auditoría, punto 8.1 — 2026-09-15): "registro de entrega" es uno de los 6 ejemplos
// documentados en 04-base-datos.md §33.
//
// usuarioId pasó de opcional a obligatorio el 2026-09-17 (P2-2/P1, docs/2026-09-17-tareas.md) —
// mismo motivo que CrearPedidoProveedorUseCase.
public sealed class CrearEntregaUseCase
{
    private readonly IEntregaRepository _entregas;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IAuditoriaRepository _auditoria;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly ILogger<CrearEntregaUseCase> _logger;

    public CrearEntregaUseCase(
        IEntregaRepository entregas, IPedidoProveedorRepository pedidos, IAuditoriaRepository auditoria,
        IGeneradorDeIdentificadores ids, ILogger<CrearEntregaUseCase> logger)
    {
        _entregas = entregas;
        _pedidos = pedidos;
        _auditoria = auditoria;
        _ids = ids;
        _logger = logger;
    }

    public EntregaResponse Ejecutar(DateTime fechaEntrega, CrearEntregaRequest request, int usuarioId)
    {
        var pedido = _pedidos.ObtenerPorId(request.PedidoProveedorId)
            ?? throw new RecursoNoEncontradoException("El pedido a proveedor indicado no existe.");

        // D-09/RN-049: NumeroRemision único dentro del pedido.
        if (_entregas.ObtenerPorPedido(pedido.Id).Any(e => e.NumeroRemision == request.NumeroRemision))
            throw new ReglaDeNegocioException(
                $"Ya existe una entrega con el número de remisión '{request.NumeroRemision}' para este pedido.");

        var entrega = new Entrega(
            id: _ids.Siguiente(),
            pedidoProveedor: pedido,
            usuarioCreacionId: usuarioId,
            fechaEntrega: fechaEntrega,
            numeroRemision: request.NumeroRemision,
            observacion: request.Observacion);

        _entregas.Guardar(entrega);

        _auditoria.Guardar(new Auditoria(
            id: _ids.Siguiente(),
            usuarioId: usuarioId,
            entidad: "Entrega",
            entidadId: entrega.Id,
            accion: "CREAR",
            fecha: fechaEntrega,
            datosNuevos: $"PedidoProveedorId={pedido.Id}; NumeroRemision={entrega.NumeroRemision}"));

        _logger.LogInformation(
            "Entrega {EntregaId} registrada (pedido {PedidoId}, remisión {NumeroRemision}) por usuario {UsuarioId}",
            entrega.Id, pedido.Id, entrega.NumeroRemision, usuarioId);

        return EntregaMapper.AResponse(entrega);
    }
}
