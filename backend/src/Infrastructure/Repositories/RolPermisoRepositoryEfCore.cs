using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class RolPermisoRepositoryEfCore : IRolPermisoRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public RolPermisoRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public bool Existe(int rolId, int permisoId) =>
        _contexto.RolesPermisos.Any(rp =>
            EF.Property<int>(rp, "RolId") == rolId && EF.Property<int>(rp, "PermisoId") == permisoId);

    public IReadOnlyList<RolPermiso> ObtenerPorRol(int rolId) =>
        _contexto.RolesPermisos
            .Include(rp => rp.Permiso)
            .Where(rp => EF.Property<int>(rp, "RolId") == rolId)
            .ToList();

    // Mismo patrón que UsuarioRolRepositoryEfCore.Guardar.
    public void Guardar(RolPermiso rolPermiso)
    {
        if (_contexto.Entry(rolPermiso).State == EntityState.Detached)
            _contexto.RolesPermisos.Add(rolPermiso);

        _contexto.SaveChanges();
    }
}
