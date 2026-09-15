using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Application.Periodos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK: habilitar las consultas necesarias para el Frontend de Requisiciones (docs/05-api.md
// §54.6), más la creación de Periodo (docs/05-api.md §16.3). RN-059/060 (punto 8, 2026-09-15):
// PERIODO_VER/CREAR, sin alcance por empresa (el periodo operativo es global, 06-seguridad.md
// §52/§53).
[ApiController]
[Route("api/v1/periodos")]
public sealed class PeriodosController : ControllerBase
{
    private readonly ListarPeriodosUseCase _listarPeriodos;
    private readonly ObtenerPeriodoUseCase _obtenerPeriodo;
    private readonly CrearPeriodoUseCase _crearPeriodo;

    public PeriodosController(
        ListarPeriodosUseCase listarPeriodos,
        ObtenerPeriodoUseCase obtenerPeriodo,
        CrearPeriodoUseCase crearPeriodo)
    {
        _listarPeriodos = listarPeriodos;
        _obtenerPeriodo = obtenerPeriodo;
        _crearPeriodo = crearPeriodo;
    }

    [Authorize(Policy = "Permiso:PERIODO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<PeriodoResponse>>> Listar()
    {
        var resultado = _listarPeriodos.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<PeriodoResponse>>.De(resultado));
    }

    // docs/05-api.md §16.2.
    [Authorize(Policy = "Permiso:PERIODO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<PeriodoResponse>> Obtener(int id)
    {
        var resultado = _obtenerPeriodo.Ejecutar(id);
        return Ok(ApiResponse<PeriodoResponse>.De(resultado));
    }

    // docs/05-api.md §16.3.
    [Authorize(Policy = "Permiso:PERIODO_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<PeriodoResponse>> Crear([FromBody] CrearPeriodoRequest request)
    {
        var resultado = _crearPeriodo.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PeriodoResponse>.De(resultado));
    }
}
