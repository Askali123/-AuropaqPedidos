using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-010 (incluye el alcance de TASK-012, adelantado por instrucción explícita del usuario).
// Usuario/Rol inexistentes -> 404 (mismo criterio que AsignarUsuarioASedeUseCase); relación
// duplicada -> 422. Rol es GLOBAL: a diferencia de UsuarioSede, no hay ninguna validación de
// "misma empresa" que aplicar aquí (no existe Empresa en Rol) — no se inventa una regla que la
// documentación no exige.
public sealed class AsignarRolAUsuarioUseCase
{
    private readonly IUsuarioRolRepository _usuariosRoles;
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;

    public AsignarRolAUsuarioUseCase(IUsuarioRolRepository usuariosRoles, IUsuarioRepository usuarios, IRolRepository roles)
    {
        _usuariosRoles = usuariosRoles;
        _usuarios = usuarios;
        _roles = roles;
    }

    public UsuarioRolResponse Ejecutar(int usuarioId, int rolId)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        var rol = _roles.ObtenerPorId(rolId)
            ?? throw new RecursoNoEncontradoException($"El rol {rolId} no existe.");

        if (_usuariosRoles.Existe(usuarioId, rolId))
            throw new ReglaDeNegocioException("El usuario ya tiene asignado ese rol.");

        var usuarioRol = new UsuarioRol(usuario, rol);

        _usuariosRoles.Guardar(usuarioRol);

        return new UsuarioRolResponse(usuarioId, rolId);
    }
}
