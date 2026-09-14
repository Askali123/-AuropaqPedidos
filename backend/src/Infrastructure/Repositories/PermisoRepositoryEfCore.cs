using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class PermisoRepositoryEfCore : IPermisoRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public PermisoRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Permiso? ObtenerPorId(int id) =>
        _contexto.Permisos.FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<Permiso> ObtenerTodos() =>
        _contexto.Permisos.OrderBy(p => p.Codigo).ToList();

    public bool ExisteParaCodigo(string codigo) =>
        _contexto.Permisos.Any(p => p.Codigo == codigo);

    // Mismo patrón que los demás repositorios (RolRepositoryEfCore.Guardar, etc.): un Permiso
    // recién creado con `new` nunca fue rastreado (Detached) y debe agregarse.
    public void Guardar(Permiso permiso)
    {
        if (_contexto.Entry(permiso).State == EntityState.Detached)
            _contexto.Permisos.Add(permiso);

        _contexto.SaveChanges();
    }
}
