using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class CategoriaRepositoryEfCore : ICategoriaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public CategoriaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Categoria? ObtenerPorId(int id) =>
        _contexto.Categorias.FirstOrDefault(c => c.Id == id);

    public IReadOnlyList<Categoria> ObtenerTodas() =>
        _contexto.Categorias.OrderBy(c => c.Nombre).ToList();

    // Mismo patrón que EmpresaRepositoryEfCore.Guardar.
    public void Guardar(Categoria categoria)
    {
        if (_contexto.Entry(categoria).State == EntityState.Detached)
            _contexto.Categorias.Add(categoria);

        _contexto.SaveChanges();
    }
}
