using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-101 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, Decisión A: endpoint de
// autoconsulta en vez de claims en el JWT). Mismo criterio ya decidido en ADR-058/
// UsuarioTienePermisoUseCase: la fuente de verdad de permisos es Usuario -> UsuarioRol -> Rol ->
// RolPermiso -> Permiso, consultada en cada llamada, sin caché ni copia en el JWT — para que el
// Frontend nunca vea permisos desactualizados si un Administrador cambia el rol de alguien a
// mitad de sesión. Reutiliza los mismos repositorios que UsuarioTienePermisoUseCase, sin crear
// una segunda fuente de permisos.
//
// Usuario inactivo -> lista vacía (no una excepción): mismo criterio de "denegación por
// defecto" ya aplicado en UsuarioTienePermisoUseCase — un JWT emitido antes de desactivar al
// usuario no debe seguir reportando permisos.
public sealed class ObtenerMisPermisosUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUsuarioRolRepository _usuariosRoles;
    private readonly IRolPermisoRepository _rolesPermisos;

    public ObtenerMisPermisosUseCase(
        IUsuarioRepository usuarios, IUsuarioRolRepository usuariosRoles, IRolPermisoRepository rolesPermisos)
    {
        _usuarios = usuarios;
        _usuariosRoles = usuariosRoles;
        _rolesPermisos = rolesPermisos;
    }

    public IReadOnlyList<PermisoResponse> Ejecutar(int usuarioId)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        if (!usuario.Activo)
            return Array.Empty<PermisoResponse>();

        return _usuariosRoles.ObtenerPorUsuario(usuarioId)
            .SelectMany(usuarioRol => _rolesPermisos.ObtenerPorRol(usuarioRol.Rol.Id))
            .Select(rolPermiso => rolPermiso.Permiso)
            .DistinctBy(permiso => permiso.Id)
            .OrderBy(permiso => permiso.Codigo)
            .Select(permiso => new PermisoResponse(permiso.Id, permiso.Codigo, permiso.Nombre, permiso.Descripcion))
            .ToList();
    }
}
