using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeRolPermisoRepository : IRolPermisoRepository
{
    private readonly List<RolPermiso> _asignaciones = new();

    public bool Existe(int rolId, int permisoId) =>
        _asignaciones.Any(rp => rp.Rol.Id == rolId && rp.Permiso.Id == permisoId);

    public IReadOnlyList<RolPermiso> ObtenerPorRol(int rolId) =>
        _asignaciones.Where(rp => rp.Rol.Id == rolId).ToList();

    public void Guardar(RolPermiso rolPermiso)
    {
        if (!_asignaciones.Contains(rolPermiso))
            _asignaciones.Add(rolPermiso);
    }
}
