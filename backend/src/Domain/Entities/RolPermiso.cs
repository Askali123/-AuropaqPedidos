using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-013, 04-base-datos.md §9.4: representa que un Rol tiene asignado un Permiso
// (Rol N --- N Permiso). Sin Id propio, mismo criterio que UsuarioRol: el documento solo lista
// RolId/PermisoId. Rol y Permiso son ambos GLOBALES (§9.1/§9.2 no declaran EmpresaId), así que
// no hay ninguna Empresa que comparar — no se inventa una regla que la documentación no exige.
public sealed class RolPermiso
{
    public Rol Rol { get; private set; }
    public Permiso Permiso { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private RolPermiso()
    {
    }
#pragma warning restore CS8618

    public RolPermiso(Rol rol, Permiso permiso)
    {
        if (rol is null)
            throw new ReglaDeNegocioException("Una asignación de permiso debe tener un rol.");

        if (permiso is null)
            throw new ReglaDeNegocioException("Una asignación de permiso debe tener un permiso.");

        Rol = rol;
        Permiso = permiso;
    }
}
