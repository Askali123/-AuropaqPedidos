using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;

namespace AuropaqPedidos.Application.PedidosProveedor;

// D-03/RN-043 (01-reglas-negocio.md §11, cierre documental 2026-09-11):
// BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO -> CANCELADO.
public sealed class CancelarPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;

    public CancelarPedidoProveedorUseCase(IPedidoProveedorRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);

        pedido.Cancelar();

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
