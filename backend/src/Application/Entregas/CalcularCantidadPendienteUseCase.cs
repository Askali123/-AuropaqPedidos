using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

namespace AuropaqPedidos.Application.Entregas;

// TASK-045, RN-034, 04-base-datos.md §30: CantidadPendiente = CantidadPedida - SUM(CantidadEntregada).
// Es una consulta derivada (no se persiste): cruza PedidoProveedor (para CantidadPedida) y
// Entrega (para la cantidad ya entregada acumulada), por eso vive en Application y no en Domain.
public sealed class CalcularCantidadPendienteUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IEntregaRepository _entregas;

    public CalcularCantidadPendienteUseCase(IPedidoProveedorRepository pedidos, IEntregaRepository entregas)
    {
        _pedidos = pedidos;
        _entregas = entregas;
    }

    public CantidadPendienteResponse Ejecutar(int pedidoProveedorId, int detallePedidoProveedorId)
    {
        var pedido = _pedidos.ObtenerPorId(pedidoProveedorId)
            ?? throw new RecursoNoEncontradoException("El pedido a proveedor indicado no existe.");

        var detallePedido = PedidoProveedorFinder.ObtenerDetalleOLanzar(pedido, detallePedidoProveedorId);

        var entregasDelPedido = _entregas.ObtenerPorPedido(pedidoProveedorId);
        var cantidadEntregada = EntregaFinder.CalcularCantidadYaEntregada(entregasDelPedido, detallePedido.Id);

        return new CantidadPendienteResponse(
            DetallePedidoProveedorId: detallePedido.Id,
            ProductoId: detallePedido.Producto.Id,
            CantidadPedida: detallePedido.CantidadPedida,
            CantidadEntregadaAcumulada: cantidadEntregada,
            CantidadPendiente: detallePedido.CantidadPedida - cantidadEntregada);
    }
}
