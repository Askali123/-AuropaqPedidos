using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-008/009/010/012, 05-api.md §55/§61/§62, ADR-061. RN-059/060 (punto 8, 2026-09-15):
// SEGURIDAD_VER protege las consultas, SEGURIDAD_ADMINISTRAR las escrituras (06-seguridad.md
// §52/§53). El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40).
[ApiController]
[Route("api/v1/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly CrearUsuarioUseCase _crearUsuario;
    private readonly ListarUsuariosUseCase _listarUsuarios;
    private readonly AsignarRolAUsuarioUseCase _asignarRol;
    private readonly ObtenerRolesDeUsuarioUseCase _obtenerRoles;
    private readonly AsignarUsuarioASedeUseCase _asignarSede;
    private readonly ObtenerSedesAutorizadasUseCase _obtenerSedes;

    public UsuariosController(
        CrearUsuarioUseCase crearUsuario,
        ListarUsuariosUseCase listarUsuarios,
        AsignarRolAUsuarioUseCase asignarRol,
        ObtenerRolesDeUsuarioUseCase obtenerRoles,
        AsignarUsuarioASedeUseCase asignarSede,
        ObtenerSedesAutorizadasUseCase obtenerSedes)
    {
        _crearUsuario = crearUsuario;
        _listarUsuarios = listarUsuarios;
        _asignarRol = asignarRol;
        _obtenerRoles = obtenerRoles;
        _asignarSede = asignarSede;
        _obtenerSedes = obtenerSedes;
    }

    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost]
    public ActionResult<ApiResponse<UsuarioResponse>> Crear([FromBody] CrearUsuarioRequest request)
    {
        var resultado = _crearUsuario.Ejecutar(request, DateTime.UtcNow);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<UsuarioResponse>>> Listar()
    {
        var resultado = _listarUsuarios.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<UsuarioResponse>>.De(resultado));
    }

    // 05-api.md §61.1. RN-060 punto 7: no permite auto-asignación (06-seguridad.md §62).
    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost("{usuarioId:int}/roles")]
    public ActionResult<ApiResponse<UsuarioRolResponse>> AsignarRol(int usuarioId, [FromBody] AsignarRolRequest request)
    {
        var resultado = _asignarRol.Ejecutar(ObtenerUsuarioIdAutenticado(), usuarioId, request.RolId, DateTime.UtcNow);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioRolResponse>.De(resultado));
    }

    // 05-api.md §61.2.
    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet("{usuarioId:int}/roles")]
    public ActionResult<ApiResponse<IReadOnlyList<RolResponse>>> ObtenerRoles(int usuarioId)
    {
        var resultado = _obtenerRoles.Ejecutar(usuarioId);
        return Ok(ApiResponse<IReadOnlyList<RolResponse>>.De(resultado));
    }

    // 05-api.md §62.1.
    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost("{usuarioId:int}/sedes")]
    public ActionResult<ApiResponse<UsuarioSedeResponse>> AsignarSede(int usuarioId, [FromBody] AsignarSedeRequest request)
    {
        var resultado = _asignarSede.Ejecutar(usuarioId, request.SedeId);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioSedeResponse>.De(resultado));
    }

    // 05-api.md §62.2.
    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet("{usuarioId:int}/sedes")]
    public ActionResult<ApiResponse<IReadOnlyList<SedeResponse>>> ObtenerSedes(int usuarioId)
    {
        var resultado = _obtenerSedes.Ejecutar(usuarioId);
        return Ok(ApiResponse<IReadOnlyList<SedeResponse>>.De(resultado));
    }

    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
