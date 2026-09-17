using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;

namespace AuropaqPedidos.Application.PedidosProveedor;

// I1-2 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): consultar el detalle de un
// pedido (detalles, distribuciones, estado).
public sealed class ObtenerPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;

    public ObtenerPedidoProveedorUseCase(IPedidoProveedorRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public PedidoProveedorResponse Ejecutar(int id)
    {
        var pedido = _pedidos.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El pedido a proveedor {id} no existe.");

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
