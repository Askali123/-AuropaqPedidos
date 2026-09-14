using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class UsuarioRolRepositoryEfCore : IUsuarioRolRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public UsuarioRolRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public bool Existe(int usuarioId, int rolId) =>
        _contexto.UsuariosRoles.Any(ur =>
            EF.Property<int>(ur, "UsuarioId") == usuarioId && EF.Property<int>(ur, "RolId") == rolId);

    public IReadOnlyList<UsuarioRol> ObtenerPorUsuario(int usuarioId) =>
        _contexto.UsuariosRoles
            .Include(ur => ur.Rol)
            .Where(ur => EF.Property<int>(ur, "UsuarioId") == usuarioId)
            .ToList();

    // Mismo patrón que UsuarioSedeRepositoryEfCore.Guardar.
    public void Guardar(UsuarioRol usuarioRol)
    {
        if (_contexto.Entry(usuarioRol).State == EntityState.Detached)
            _contexto.UsuariosRoles.Add(usuarioRol);

        _contexto.SaveChanges();
    }
}
