using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-010, 04-base-datos.md §9.3: representa que un Usuario tiene asignado un Rol
// (Usuario N --- N Rol). Sin Id propio, mismo criterio que UsuarioSede: el documento solo lista
// UsuarioId/RolId. A diferencia de UsuarioSede, esta relación NO valida pertenencia a la misma
// empresa: Rol es global (§9.1 no declara EmpresaId), así que no hay ninguna Empresa que
// comparar — no se inventa una regla que la documentación no exige aquí.
public sealed class UsuarioRol
{
    public Usuario Usuario { get; private set; }
    public Rol Rol { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private UsuarioRol()
    {
    }
#pragma warning restore CS8618

    public UsuarioRol(Usuario usuario, Rol rol)
    {
        if (usuario is null)
            throw new ReglaDeNegocioException("Una asignación de rol debe tener un usuario.");

        if (rol is null)
            throw new ReglaDeNegocioException("Una asignación de rol debe tener un rol.");

        Usuario = usuario;
        Rol = rol;
    }
}
