using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeEntregaRepository : IEntregaRepository
{
    private readonly Dictionary<int, Entrega> _entregas = new();

    public Entrega? ObtenerPorId(int id) => _entregas.TryGetValue(id, out var entrega) ? entrega : null;

    public IReadOnlyList<Entrega> ObtenerPorPedido(int pedidoProveedorId) =>
        _entregas.Values.Where(e => e.PedidoProveedor.Id == pedidoProveedorId).ToList();

    public void Guardar(Entrega entrega) => _entregas[entrega.Id] = entrega;
}
