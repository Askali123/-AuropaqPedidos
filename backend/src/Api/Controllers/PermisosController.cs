using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-011, 05-api.md §60, ADR-061. RN-059/060 (punto 8, 2026-09-15): SEGURIDAD_VER/ADMINISTRAR
// (06-seguridad.md §52/§53).
[ApiController]
[Route("api/v1/permisos")]
public sealed class PermisosController : ControllerBase
{
    private readonly ListarPermisosUseCase _listarPermisos;
    private readonly CrearPermisoUseCase _crearPermiso;

    public PermisosController(ListarPermisosUseCase listarPermisos, CrearPermisoUseCase crearPermiso)
    {
        _listarPermisos = listarPermisos;
        _crearPermiso = crearPermiso;
    }

    // 05-api.md §60.1.
    [Authorize(Policy = "Permiso:SEGURIDAD_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<PermisoResponse>>> Listar()
    {
        var resultado = _listarPermisos.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<PermisoResponse>>.De(resultado));
    }

    // 05-api.md §60.2.
    [Authorize(Policy = "Permiso:SEGURIDAD_ADMINISTRAR")]
    [HttpPost]
    public ActionResult<ApiResponse<PermisoResponse>> Crear([FromBody] CrearPermisoRequest request)
    {
        var resultado = _crearPermiso.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PermisoResponse>.De(resultado));
    }
}
