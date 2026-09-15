using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-010/013, 05-api.md §59/§63, ADR-061. RN-059/060 (punto 8, 2026-09-15): SEGURIDAD_VER
// protege las consultas, SEGURIDAD_ADMINISTRAR las escrituras (06-seguridad.md §52/§53).
[ApiController]
[Route("api/v1/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ListarRolesUseCase _listarRoles;
    private readonly CrearRolUseCase _crearRol;
    private readonly AsignarPermisoARolUseCase _asignarPermiso;
    private readonly ObtenerPermisosDeRolUseCase _obtenerPermisos;

    public RolesController(
        ListarRolesUseCase listarRoles,
        CrearRolUseCase crearRol,
        AsignarPermisoARolUseCase asignarPermiso,
        ObtenerPermisosDeRolUseCase obtenerPermisos)
    {
        _listarRoles = listarRoles;
        _crearRol = crearRol;
        _asignarPermiso = asignarPermiso;
        _obtenerPermisos = obtenerPermisos;
    }

    // 05-api.md §59.1.
    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<RolResponse>>> Listar()
    {
        var resultado = _listarRoles.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<RolResponse>>.De(resultado));
    }

    // 05-api.md §59.2.
    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost]
    public ActionResult<ApiResponse<RolResponse>> Crear([FromBody] CrearRolRequest request)
    {
        var resultado = _crearRol.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolResponse>.De(resultado));
    }

    // 05-api.md §63.1.
    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost("{rolId:int}/permisos")]
    public ActionResult<ApiResponse<RolPermisoResponse>> AsignarPermiso(int rolId, [FromBody] AsignarPermisoRequest request)
    {
        var resultado = _asignarPermiso.Ejecutar(rolId, request.PermisoId);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolPermisoResponse>.De(resultado));
    }

    // 05-api.md §63.2.
    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet("{rolId:int}/permisos")]
    public ActionResult<ApiResponse<IReadOnlyList<PermisoResponse>>> ObtenerPermisos(int rolId)
    {
        var resultado = _obtenerPermisos.Ejecutar(rolId);
        return Ok(ApiResponse<IReadOnlyList<PermisoResponse>>.De(resultado));
    }
}
