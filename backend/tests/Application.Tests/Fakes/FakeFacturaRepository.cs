using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeFacturaRepository : IFacturaRepository
{
    private readonly Dictionary<int, Factura> _facturas = new();

    public Factura? ObtenerPorId(int id) => _facturas.TryGetValue(id, out var factura) ? factura : null;

    public IReadOnlyList<Factura> ObtenerPorPedido(int pedidoProveedorId) =>
        _facturas.Values.Where(f => f.PedidoProveedor.Id == pedidoProveedorId).ToList();

    public bool ExisteNumeroFacturaParaProveedor(int proveedorId, string numeroFactura) =>
        _facturas.Values.Any(f => f.Proveedor.Id == proveedorId && f.NumeroFactura == numeroFactura);

    public void Guardar(Factura factura) => _facturas[factura.Id] = factura;
}
