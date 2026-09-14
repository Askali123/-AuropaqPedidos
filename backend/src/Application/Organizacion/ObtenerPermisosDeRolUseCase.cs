using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-013: satisface "Rol -> Permisos" (08-tareas.md TASK-013). Rol inexistente -> 404, mismo
// criterio que ObtenerRolesDeUsuarioUseCase.
public sealed class ObtenerPermisosDeRolUseCase
{
    private readonly IRolRepository _roles;
    private readonly IRolPermisoRepository _rolesPermisos;

    public ObtenerPermisosDeRolUseCase(IRolRepository roles, IRolPermisoRepository rolesPermisos)
    {
        _roles = roles;
        _rolesPermisos = rolesPermisos;
    }

    public IReadOnlyList<PermisoResponse> Ejecutar(int rolId)
    {
        _ = _roles.ObtenerPorId(rolId)
            ?? throw new RecursoNoEncontradoException($"El rol {rolId} no existe.");

        return _rolesPermisos.ObtenerPorRol(rolId)
            .Select(rp => new PermisoResponse(rp.Permiso.Id, rp.Permiso.Codigo, rp.Permiso.Nombre, rp.Permiso.Descripcion))
            .ToList();
    }
}
