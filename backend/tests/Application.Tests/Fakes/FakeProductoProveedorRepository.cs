using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeProductoProveedorRepository : IProductoProveedorRepository
{
    private readonly Dictionary<int, ProductoProveedor> _relaciones = new();

    public ProductoProveedor? ObtenerPorId(int id) => _relaciones.TryGetValue(id, out var relacion) ? relacion : null;

    public bool Existe(int productoId, int proveedorId) =>
        _relaciones.Values.Any(r => r.Producto.Id == productoId && r.Proveedor.Id == proveedorId);

    public IReadOnlyList<ProductoProveedor> ObtenerPorProducto(int productoId) =>
        _relaciones.Values.Where(r => r.Producto.Id == productoId).ToList();

    public IReadOnlyList<ProductoProveedor> ObtenerPorProveedor(int proveedorId) =>
        _relaciones.Values.Where(r => r.Proveedor.Id == proveedorId).ToList();

    public void Guardar(ProductoProveedor productoProveedor) => _relaciones[productoProveedor.Id] = productoProveedor;
}
