using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class ProductoProveedorRepositoryEfCore : IProductoProveedorRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public ProductoProveedorRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public ProductoProveedor? ObtenerPorId(int id) =>
        _contexto.ProductosProveedores
            .Include(pp => pp.Producto)
            .Include(pp => pp.Proveedor)
            .FirstOrDefault(pp => pp.Id == id);

    public bool Existe(int productoId, int proveedorId) =>
        _contexto.ProductosProveedores.Any(pp =>
            EF.Property<int>(pp, "ProductoId") == productoId && EF.Property<int>(pp, "ProveedorId") == proveedorId);

    public IReadOnlyList<ProductoProveedor> ObtenerPorProducto(int productoId) =>
        _contexto.ProductosProveedores
            .Include(pp => pp.Producto)
            .Include(pp => pp.Proveedor)
            .Where(pp => EF.Property<int>(pp, "ProductoId") == productoId)
            .ToList();

    public IReadOnlyList<ProductoProveedor> ObtenerPorProveedor(int proveedorId) =>
        _contexto.ProductosProveedores
            .Include(pp => pp.Producto)
            .Include(pp => pp.Proveedor)
            .Where(pp => EF.Property<int>(pp, "ProveedorId") == proveedorId)
            .ToList();

    // Mismo patrón que ProductoRepositoryEfCore.Guardar.
    public void Guardar(ProductoProveedor productoProveedor)
    {
        if (_contexto.Entry(productoProveedor).State == EntityState.Detached)
            _contexto.ProductosProveedores.Add(productoProveedor);

        _contexto.SaveChanges();
    }
}
