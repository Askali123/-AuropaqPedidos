using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class ProveedorRepositoryEfCore : IProveedorRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public ProveedorRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Proveedor? ObtenerPorId(int id) =>
        _contexto.Proveedores.FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<Proveedor> ObtenerTodos() =>
        _contexto.Proveedores.OrderBy(p => p.Nombre).ToList();

    // Mismo patrón que EmpresaRepositoryEfCore.Guardar.
    public void Guardar(Proveedor proveedor)
    {
        if (_contexto.Entry(proveedor).State == EntityState.Detached)
            _contexto.Proveedores.Add(proveedor);

        _contexto.SaveChanges();
    }
}
