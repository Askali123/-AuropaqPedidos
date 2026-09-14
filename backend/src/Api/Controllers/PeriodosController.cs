using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Application.Periodos.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK: habilitar las consultas necesarias para el Frontend de Requisiciones (docs/05-api.md
// §54.6), más la creación de Periodo (docs/05-api.md §16.3) para poder demostrar en vivo el
// flujo completo de estados de Requisición sin depender de fechas ya vencidas.
[ApiController]
[Route("api/v1/periodos")]
public sealed class PeriodosController : ControllerBase
{
    private readonly ListarPeriodosUseCase _listarPeriodos;
    private readonly CrearPeriodoUseCase _crearPeriodo;

    public PeriodosController(ListarPeriodosUseCase listarPeriodos, CrearPeriodoUseCase crearPeriodo)
    {
        _listarPeriodos = listarPeriodos;
        _crearPeriodo = crearPeriodo;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<PeriodoResponse>>> Listar()
    {
        var resultado = _listarPeriodos.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<PeriodoResponse>>.De(resultado));
    }

    // docs/05-api.md §16.3. Sin header de identidad: ningún documento define que crear un
    // Periodo dependa de un usuario/empresa concreto (a diferencia de Requisicion), así que no
    // se exige uno que no está en el contrato documentado.
    [HttpPost]
    public ActionResult<ApiResponse<PeriodoResponse>> Crear([FromBody] CrearPeriodoRequest request)
    {
        var resultado = _crearPeriodo.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PeriodoResponse>.De(resultado));
    }
}
