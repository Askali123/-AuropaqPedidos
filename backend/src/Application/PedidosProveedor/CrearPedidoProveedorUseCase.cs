using AuropaqPedidos.Application.Auditorias.Abstracciones;
using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.PedidosProveedor;

// TASK-039, RN-030/RN-031: crea un PedidoProveedor vacío (sin detalles) asociado a una
// Consolidacion existente y a un Proveedor. ProveedorId y NumeroPedido se reciben explícitos
// del llamador: no existe una regla documentada de selección automática de proveedor ni de
// generación de número de pedido (auditoría 2026-09-10, Pendientes 7/9).
//
// TASK-056 (Auditoría, punto 8.1 — 2026-09-15): "creación de pedido" es uno de los 6 ejemplos
// documentados en 04-base-datos.md §33. usuarioId es nullable porque este endpoint todavía no
// exige autenticación real (a diferencia de Requisición) — se registra cuando el llamador
// incluye un JWT válido, aunque no sea obligatorio.
public sealed class CrearPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IConsolidacionRepository _consolidaciones;
    private readonly IProveedorRepository _proveedores;
    private readonly IAuditoriaRepository _auditoria;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly ILogger<CrearPedidoProveedorUseCase> _logger;

    public CrearPedidoProveedorUseCase(
        IPedidoProveedorRepository pedidos,
        IConsolidacionRepository consolidaciones,
        IProveedorRepository proveedores,
        IAuditoriaRepository auditoria,
        IGeneradorDeIdentificadores ids,
        ILogger<CrearPedidoProveedorUseCase> logger)
    {
        _pedidos = pedidos;
        _consolidaciones = consolidaciones;
        _proveedores = proveedores;
        _auditoria = auditoria;
        _ids = ids;
        _logger = logger;
    }

    public PedidoProveedorResponse Ejecutar(DateTime fechaPedido, CrearPedidoProveedorRequest request, int? usuarioId = null)
    {
        var consolidacion = _consolidaciones.ObtenerPorId(request.ConsolidacionId)
            ?? throw new RecursoNoEncontradoException("La consolidación indicada no existe.");

        var proveedor = _proveedores.ObtenerPorId(request.ProveedorId)
            ?? throw new RecursoNoEncontradoException("El proveedor indicado no existe.");

        // Mismo criterio que "producto activo"/"sede activa" en Requisiciones: Domain solo
        // exige que el proveedor no sea null, así que "activo" se valida aquí.
        if (!proveedor.Activo)
            throw new ReglaDeNegocioException("El proveedor no está activo.");

        // D-09/RN-049: NumeroPedido único dentro del proveedor.
        if (_pedidos.ExisteNumeroPedidoParaProveedor(proveedor.Id, request.NumeroPedido))
            throw new ReglaDeNegocioException(
                $"Ya existe un pedido con el número '{request.NumeroPedido}' para este proveedor.");

        var pedido = new PedidoProveedor(
            id: _ids.Siguiente(),
            consolidacion: consolidacion,
            proveedor: proveedor,
            numeroPedido: request.NumeroPedido,
            fechaPedido: fechaPedido,
            fechaEntregaEstimada: request.FechaEntregaEstimada,
            observacion: request.Observacion);

        _pedidos.Guardar(pedido);

        _auditoria.Guardar(new Auditoria(
            id: _ids.Siguiente(),
            usuarioId: usuarioId,
            entidad: "PedidoProveedor",
            entidadId: pedido.Id,
            accion: "CREAR",
            fecha: fechaPedido,
            datosNuevos: $"ProveedorId={proveedor.Id}; NumeroPedido={pedido.NumeroPedido}; ConsolidacionId={consolidacion.Id}"));

        _logger.LogInformation(
            "PedidoProveedor {PedidoId} creado (proveedor {ProveedorId}, consolidación {ConsolidacionId}) por usuario {UsuarioId}",
            pedido.Id, proveedor.Id, consolidacion.Id, usuarioId);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
