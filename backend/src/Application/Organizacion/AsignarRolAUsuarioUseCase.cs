using AuropaqPedidos.Application.Auditorias.Abstracciones;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-010 (incluye el alcance de TASK-012, adelantado por instrucción explícita del usuario).
// Usuario/Rol inexistentes -> 404 (mismo criterio que AsignarUsuarioASedeUseCase); relación
// duplicada -> 422. Rol es GLOBAL: a diferencia de UsuarioSede, no hay ninguna validación de
// "misma empresa" que aplicar aquí (no existe Empresa en Rol) — no se inventa una regla que la
// documentación no exige.
// RN-060 punto 7 (06-seguridad.md §62, prevención de escalamiento de privilegios): ningún
// usuario puede asignarse un rol a sí mismo, ni siquiera con SEGURIDAD_ADMINISTRAR.
//
// TASK-056 (Auditoría, punto 8.1 — 2026-09-15): "cambios administrativos" (04-base-datos.md
// §33) — asignar un rol es el cambio administrativo más sensible ya modelado (RBAC), por eso se
// elige este punto en concreto en vez de auditar todo el módulo de Seguridad.
public sealed class AsignarRolAUsuarioUseCase
{
    private readonly IUsuarioRolRepository _usuariosRoles;
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;
    private readonly IAuditoriaRepository _auditoria;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly ILogger<AsignarRolAUsuarioUseCase> _logger;

    public AsignarRolAUsuarioUseCase(
        IUsuarioRolRepository usuariosRoles, IUsuarioRepository usuarios, IRolRepository roles,
        IAuditoriaRepository auditoria, IGeneradorDeIdentificadores ids, ILogger<AsignarRolAUsuarioUseCase> logger)
    {
        _usuariosRoles = usuariosRoles;
        _usuarios = usuarios;
        _roles = roles;
        _auditoria = auditoria;
        _ids = ids;
        _logger = logger;
    }

    public UsuarioRolResponse Ejecutar(int usuarioIdActor, int usuarioId, int rolId, DateTime fecha)
    {
        if (usuarioIdActor == usuarioId)
            throw new ReglaDeNegocioException("Un usuario no puede asignarse un rol a sí mismo.");

        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        var rol = _roles.ObtenerPorId(rolId)
            ?? throw new RecursoNoEncontradoException($"El rol {rolId} no existe.");

        if (_usuariosRoles.Existe(usuarioId, rolId))
            throw new ReglaDeNegocioException("El usuario ya tiene asignado ese rol.");

        var usuarioRol = new UsuarioRol(usuario, rol);

        _usuariosRoles.Guardar(usuarioRol);

        _auditoria.Guardar(new Auditoria(
            id: _ids.Siguiente(),
            usuarioId: usuarioIdActor,
            entidad: "Usuario",
            entidadId: usuarioId,
            accion: "ASIGNAR_ROL",
            fecha: fecha,
            datosNuevos: $"RolId={rolId}"));

        _logger.LogInformation(
            "Rol {RolId} asignado al usuario {UsuarioId} por el usuario {UsuarioIdActor}",
            rolId, usuarioId, usuarioIdActor);

        return new UsuarioRolResponse(usuarioId, rolId);
    }
}
