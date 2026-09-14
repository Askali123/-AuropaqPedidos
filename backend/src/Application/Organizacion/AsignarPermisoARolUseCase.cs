using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-013. Rol/Permiso inexistentes -> 404 (mismo criterio que AsignarRolAUsuarioUseCase);
// relación duplicada -> 422. Rol y Permiso son GLOBALES: no hay ninguna validación de "misma
// empresa" que aplicar aquí (ninguno de los dos declara EmpresaId) — no se inventa una regla que
// la documentación no exige.
public sealed class AsignarPermisoARolUseCase
{
    private readonly IRolPermisoRepository _rolesPermisos;
    private readonly IRolRepository _roles;
    private readonly IPermisoRepository _permisos;

    public AsignarPermisoARolUseCase(IRolPermisoRepository rolesPermisos, IRolRepository roles, IPermisoRepository permisos)
    {
        _rolesPermisos = rolesPermisos;
        _roles = roles;
        _permisos = permisos;
    }

    public RolPermisoResponse Ejecutar(int rolId, int permisoId)
    {
        var rol = _roles.ObtenerPorId(rolId)
            ?? throw new RecursoNoEncontradoException($"El rol {rolId} no existe.");

        var permiso = _permisos.ObtenerPorId(permisoId)
            ?? throw new RecursoNoEncontradoException($"El permiso {permisoId} no existe.");

        if (_rolesPermisos.Existe(rolId, permisoId))
            throw new ReglaDeNegocioException("El rol ya tiene asignado ese permiso.");

        var rolPermiso = new RolPermiso(rol, permiso);

        _rolesPermisos.Guardar(rolPermiso);

        return new RolPermisoResponse(rolId, permisoId);
    }
}
