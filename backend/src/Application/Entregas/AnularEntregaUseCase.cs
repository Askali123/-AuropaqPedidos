using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Enums;

namespace AuropaqPedidos.Application.Entregas;

// D-04/RN-046 (01-reglas-negocio.md §12) + B1 (cierre técnico 2026-09-11): REGISTRADA -> ANULADA.
// Domain (Entrega.Anular) ya rechaza anular si el PedidoProveedor está CERRADO. Tras anular,
// recalcula PedidoProveedor.Estado según las entregas VÁLIDAS restantes (mismo cálculo
// compartido que AgregarDetalleEntregaUseCase, vía EntregaFinder.CalcularEstadoAgregadoDeEntregas
// — que ya excluye entregas ANULADAS). Si el pedido quedó CANCELADO mientras tenía entregas
// registradas (posible: Cancelar() es válido desde PARCIALMENTE_ENTREGADO), no tiene sentido
// recalcular su estado — se anula la entrega igual, pero no se toca PedidoProveedor.Estado.
public sealed class AnularEntregaUseCase
{
    private readonly IEntregaRepository _entregas;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly ITransaccionDeEntrega _transaccion;

    public AnularEntregaUseCase(IEntregaRepository entregas, IPedidoProveedorRepository pedidos, ITransaccionDeEntrega transaccion)
    {
        _entregas = entregas;
        _pedidos = pedidos;
        _transaccion = transaccion;
    }

    public EntregaResponse Ejecutar(int entregaId)
    {
        var entrega = EntregaFinder.ObtenerOLanzar(_entregas, entregaId);

        // Auditoría (problema crítico de atomicidad): igual que en AgregarDetalleEntregaUseCase,
        // anular la Entrega y recalcular PedidoProveedor.Estado deben confirmarse juntos.
        _transaccion.Ejecutar(() =>
        {
            entrega.Anular();

            _entregas.Guardar(entrega);

            var pedido = entrega.PedidoProveedor;
            if (pedido.Estado is PedidoProveedorEstado.Enviado or PedidoProveedorEstado.ParcialmenteEntregado or PedidoProveedorEstado.Entregado)
            {
                var entregasDelPedido = _entregas.ObtenerPorPedido(pedido.Id);
                var (hayAlgunaCantidadEntregada, quedaCantidadPendiente) =
                    EntregaFinder.CalcularEstadoAgregadoDeEntregas(pedido, entregasDelPedido);

                pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada, quedaCantidadPendiente);
                _pedidos.Guardar(pedido);
            }
        });

        return EntregaMapper.AResponse(entrega);
    }
}
