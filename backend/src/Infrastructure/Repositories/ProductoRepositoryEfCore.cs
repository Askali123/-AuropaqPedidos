using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class ProductoRepositoryEfCore : IProductoRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public ProductoRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Producto? ObtenerPorId(int id) =>
        _contexto.Productos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<Producto> ObtenerTodos() =>
        _contexto.Productos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .OrderBy(p => p.Nombre)
            .ToList();
}
