using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class RolRepositoryEfCore : IRolRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public RolRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Rol? ObtenerPorId(int id) =>
        _contexto.Roles.FirstOrDefault(r => r.Id == id);

    public IReadOnlyList<Rol> ObtenerTodos() =>
        _contexto.Roles.OrderBy(r => r.Nombre).ToList();

    // Mismo patrón que los demás repositorios (RequisicionRepositoryEfCore.Guardar, etc.): un
    // Rol recién creado con `new` nunca fue rastreado (Detached) y debe agregarse.
    public void Guardar(Rol rol)
    {
        if (_contexto.Entry(rol).State == EntityState.Detached)
            _contexto.Roles.Add(rol);

        _contexto.SaveChanges();
    }
}
