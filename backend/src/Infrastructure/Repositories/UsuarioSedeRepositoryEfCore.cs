using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class UsuarioSedeRepositoryEfCore : IUsuarioSedeRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public UsuarioSedeRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public bool Existe(int usuarioId, int sedeId) =>
        _contexto.UsuariosSedes.Any(us =>
            EF.Property<int>(us, "UsuarioId") == usuarioId && EF.Property<int>(us, "SedeId") == sedeId);

    public IReadOnlyList<UsuarioSede> ObtenerPorUsuario(int usuarioId) =>
        _contexto.UsuariosSedes
            .Include(us => us.Sede)
            .Where(us => EF.Property<int>(us, "UsuarioId") == usuarioId)
            .ToList();

    // Mismo patrón que los demás repositorios (RequisicionRepositoryEfCore.Guardar, etc.): una
    // relación recién creada con `new` nunca fue rastreada (Detached) y debe agregarse.
    public void Guardar(UsuarioSede usuarioSede)
    {
        if (_contexto.Entry(usuarioSede).State == EntityState.Detached)
            _contexto.UsuariosSedes.Add(usuarioSede);

        _contexto.SaveChanges();
    }
}
