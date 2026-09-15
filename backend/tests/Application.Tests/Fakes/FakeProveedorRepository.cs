using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeProveedorRepository : IProveedorRepository
{
    private readonly Dictionary<int, Proveedor> _proveedores = new();

    public void Agregar(Proveedor proveedor) => _proveedores[proveedor.Id] = proveedor;

    public Proveedor? ObtenerPorId(int id) => _proveedores.TryGetValue(id, out var proveedor) ? proveedor : null;

    public IReadOnlyList<Proveedor> ObtenerTodos() => _proveedores.Values.OrderBy(p => p.Nombre).ToList();

    public void Guardar(Proveedor proveedor) => _proveedores[proveedor.Id] = proveedor;
}
