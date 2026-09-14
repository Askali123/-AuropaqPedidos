using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-010/012: satisface "un usuario puede tener uno o varios roles" (08-tareas.md TASK-012).
// Usuario inexistente -> 404 (mismo criterio que ObtenerSedesAutorizadasUseCase).
public sealed class ObtenerRolesDeUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUsuarioRolRepository _usuariosRoles;

    public ObtenerRolesDeUsuarioUseCase(IUsuarioRepository usuarios, IUsuarioRolRepository usuariosRoles)
    {
        _usuarios = usuarios;
        _usuariosRoles = usuariosRoles;
    }

    public IReadOnlyList<RolResponse> Ejecutar(int usuarioId)
    {
        _ = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        return _usuariosRoles.ObtenerPorUsuario(usuarioId)
            .Select(ur => new RolResponse(ur.Rol.Id, ur.Rol.Nombre, ur.Rol.Descripcion, ur.Rol.Activo))
            .ToList();
    }
}
