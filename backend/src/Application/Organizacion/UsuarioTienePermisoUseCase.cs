using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-016. Fuente de verdad: Usuario -> UsuarioRol -> Rol -> RolPermiso -> Permiso.Codigo,
// consultada dinámicamente en cada llamada (04-base-datos.md §9.4, TASK-016 §10: "no confiar en
// que el JWT contiene una copia permanente de todos los permisos"). No se agregó ninguna
// abstracción/tabla nueva: reutiliza IUsuarioRepository/IUsuarioRolRepository/IRolPermisoRepository
// ya existentes (TASK-016 §3/§25: no crear una segunda fuente de permisos).
//
// También exige Usuario.Activo == true (mismo criterio ya cerrado en RN-056/LoginUseCase para
// autenticación, TASK-015) — no es una regla nueva inventada aquí, es la misma regla de
// "denegación por defecto" aplicada también en autorización: un JWT emitido antes de desactivar
// al usuario no debe seguir otorgando acceso indefinidamente mientras el token no expire
// (TASK-016 §11). No implementa revocación de tokens; solo evita que un usuario inactivo, aunque
// su JWT siga siendo válido criptográficamente, pueda pasar la verificación de permisos.
public sealed class UsuarioTienePermisoUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUsuarioRolRepository _usuariosRoles;
    private readonly IRolPermisoRepository _rolesPermisos;

    public UsuarioTienePermisoUseCase(
        IUsuarioRepository usuarios,
        IUsuarioRolRepository usuariosRoles,
        IRolPermisoRepository rolesPermisos)
    {
        _usuarios = usuarios;
        _usuariosRoles = usuariosRoles;
        _rolesPermisos = rolesPermisos;
    }

    public bool Ejecutar(int usuarioId, string permisoCodigo)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId);
        if (usuario is null || !usuario.Activo)
            return false;

        foreach (var usuarioRol in _usuariosRoles.ObtenerPorUsuario(usuarioId))
        {
            var tienePermiso = _rolesPermisos.ObtenerPorRol(usuarioRol.Rol.Id)
                .Any(rolPermiso => rolPermiso.Permiso.Codigo == permisoCodigo);

            if (tienePermiso)
                return true;
        }

        return false;
    }
}
