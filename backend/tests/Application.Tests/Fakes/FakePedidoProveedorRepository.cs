using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakePedidoProveedorRepository : IPedidoProveedorRepository
{
    private readonly Dictionary<int, PedidoProveedor> _pedidos = new();

    public PedidoProveedor? ObtenerPorId(int id) => _pedidos.TryGetValue(id, out var pedido) ? pedido : null;

    public DetallePedidoProveedor? ObtenerDetallePorId(int detalleId) =>
        _pedidos.Values.SelectMany(p => p.Detalles).FirstOrDefault(d => d.Id == detalleId);

    public bool ExisteNumeroPedidoParaProveedor(int proveedorId, string numeroPedido) =>
        _pedidos.Values.Any(p => p.Proveedor.Id == proveedorId && p.NumeroPedido == numeroPedido);

    public void Guardar(PedidoProveedor pedido) => _pedidos[pedido.Id] = pedido;
}
