using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;

namespace AuropaqPedidos.Application.PedidosProveedor;

// D-03/RN-043 (01-reglas-negocio.md §11, cierre documental 2026-09-11): BORRADOR -> ENVIADO.
// Sin esta transición un pedido nunca podría recibir entregas (Entrega exige pedido
// ENVIADO/PARCIALMENTE_ENTREGADO).
public sealed class EnviarPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;

    public EnviarPedidoProveedorUseCase(IPedidoProveedorRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);

        pedido.Enviar();

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
