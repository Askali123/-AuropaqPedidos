using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Entregas;

// TASK-042, RN-033: registra una entrega (posiblemente parcial) asociada a un PedidoProveedor
// existente. NumeroRemision se recibe explícito del llamador (mismo criterio que
// PedidoProveedor.NumeroPedido).
public sealed class CrearEntregaUseCase
{
    private readonly IEntregaRepository _entregas;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearEntregaUseCase(IEntregaRepository entregas, IPedidoProveedorRepository pedidos, IGeneradorDeIdentificadores ids)
    {
        _entregas = entregas;
        _pedidos = pedidos;
        _ids = ids;
    }

    public EntregaResponse Ejecutar(DateTime fechaEntrega, CrearEntregaRequest request)
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
            fechaEntrega: fechaEntrega,
            numeroRemision: request.NumeroRemision,
            observacion: request.Observacion);

        _entregas.Guardar(entrega);

        return EntregaMapper.AResponse(entrega);
    }
}
