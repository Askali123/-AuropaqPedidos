using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK: habilitar las consultas necesarias para el Frontend de Requisiciones (docs/05-api.md
// §54.6). Solo lectura del catálogo existente (sin CRUD de Producto en esta tarea).
[ApiController]
[Route("api/v1/productos")]
public sealed class ProductosController : ControllerBase
{
    private readonly ListarProductosUseCase _listarProductos;

    public ProductosController(ListarProductosUseCase listarProductos)
    {
        _listarProductos = listarProductos;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<ProductoResponse>>> Listar()
    {
        var resultado = _listarProductos.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<ProductoResponse>>.De(resultado));
    }
}
