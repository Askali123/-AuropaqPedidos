using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-014, 05-api.md §14. El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40).
// RN-059/060 (punto 8, 2026-09-15): PRODUCTO_VER/CREAR/EDITAR (Categoría se agrupa bajo el
// permiso de Producto, 06-seguridad.md §52/§53 — sin ciclo de vida propio).
[ApiController]
[Route("api/v1/categorias")]
public sealed class CategoriasController : ControllerBase
{
    private readonly ListarCategoriasUseCase _listarCategorias;
    private readonly CrearCategoriaUseCase _crearCategoria;
    private readonly ObtenerCategoriaUseCase _obtenerCategoria;
    private readonly ActualizarCategoriaUseCase _actualizarCategoria;

    public CategoriasController(
        ListarCategoriasUseCase listarCategorias,
        CrearCategoriaUseCase crearCategoria,
        ObtenerCategoriaUseCase obtenerCategoria,
        ActualizarCategoriaUseCase actualizarCategoria)
    {
        _listarCategorias = listarCategorias;
        _crearCategoria = crearCategoria;
        _obtenerCategoria = obtenerCategoria;
        _actualizarCategoria = actualizarCategoria;
    }

    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<CategoriaResponse>>> Listar()
    {
        var resultado = _listarCategorias.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<CategoriaResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<CategoriaResponse>> Obtener(int id)
    {
        var resultado = _obtenerCategoria.Ejecutar(id);
        return Ok(ApiResponse<CategoriaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<CategoriaResponse>> Crear([FromBody] CrearCategoriaRequest request)
    {
        var resultado = _crearCategoria.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CategoriaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_EDITAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<CategoriaResponse>> Actualizar(int id, [FromBody] ActualizarCategoriaRequest request)
    {
        var resultado = _actualizarCategoria.Ejecutar(id, request);
        return Ok(ApiResponse<CategoriaResponse>.De(resultado));
    }
}
