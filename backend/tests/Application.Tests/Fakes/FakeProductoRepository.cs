using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeProductoRepository : IProductoRepository
{
    private readonly Dictionary<int, Producto> _productos = new();

    public void Agregar(Producto producto) => _productos[producto.Id] = producto;

    public Producto? ObtenerPorId(int id) => _productos.TryGetValue(id, out var producto) ? producto : null;

    public IReadOnlyList<Producto> ObtenerTodos() => _productos.Values.OrderBy(p => p.Nombre).ToList();

    public void Guardar(Producto producto) => _productos[producto.Id] = producto;
}
