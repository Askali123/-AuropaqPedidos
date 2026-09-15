using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-015, 05-api.md §15. El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40).
// RN-059/060 (punto 8, 2026-09-15): PRODUCTO_VER/CREAR/EDITAR (UnidadMedida se agrupa bajo el
// permiso de Producto, 06-seguridad.md §52/§53 — sin ciclo de vida propio).
[ApiController]
[Route("api/v1/unidades-medida")]
public sealed class UnidadesMedidaController : ControllerBase
{
    private readonly ListarUnidadesMedidaUseCase _listarUnidadesMedida;
    private readonly CrearUnidadMedidaUseCase _crearUnidadMedida;
    private readonly ObtenerUnidadMedidaUseCase _obtenerUnidadMedida;
    private readonly ActualizarUnidadMedidaUseCase _actualizarUnidadMedida;

    public UnidadesMedidaController(
        ListarUnidadesMedidaUseCase listarUnidadesMedida,
        CrearUnidadMedidaUseCase crearUnidadMedida,
        ObtenerUnidadMedidaUseCase obtenerUnidadMedida,
        ActualizarUnidadMedidaUseCase actualizarUnidadMedida)
    {
        _listarUnidadesMedida = listarUnidadesMedida;
        _crearUnidadMedida = crearUnidadMedida;
        _obtenerUnidadMedida = obtenerUnidadMedida;
        _actualizarUnidadMedida = actualizarUnidadMedida;
    }

    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<UnidadMedidaResponse>>> Listar()
    {
        var resultado = _listarUnidadesMedida.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<UnidadMedidaResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<UnidadMedidaResponse>> Obtener(int id)
    {
        var resultado = _obtenerUnidadMedida.Ejecutar(id);
        return Ok(ApiResponse<UnidadMedidaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<UnidadMedidaResponse>> Crear([FromBody] CrearUnidadMedidaRequest request)
    {
        var resultado = _crearUnidadMedida.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UnidadMedidaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_EDITAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<UnidadMedidaResponse>> Actualizar(int id, [FromBody] ActualizarUnidadMedidaRequest request)
    {
        var resultado = _actualizarUnidadMedida.Ejecutar(id, request);
        return Ok(ApiResponse<UnidadMedidaResponse>.De(resultado));
    }
}
