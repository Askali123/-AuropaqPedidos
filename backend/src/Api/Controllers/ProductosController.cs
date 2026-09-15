using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-016, 05-api.md §13. El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40).
// RN-059/060 (punto 8, 2026-09-15): PRODUCTO_VER/CREAR/EDITAR (06-seguridad.md §52/§53).
[ApiController]
[Route("api/v1/productos")]
public sealed class ProductosController : ControllerBase
{
    private readonly ListarProductosUseCase _listarProductos;
    private readonly CrearProductoUseCase _crearProducto;
    private readonly ObtenerProductoUseCase _obtenerProducto;
    private readonly ActualizarProductoUseCase _actualizarProducto;
    private readonly ListarProveedoresDeProductoUseCase _listarProveedores;
    private readonly AsociarProveedorAProductoUseCase _asociarProveedor;
    private readonly ActualizarProductoProveedorUseCase _actualizarProductoProveedor;

    public ProductosController(
        ListarProductosUseCase listarProductos,
        CrearProductoUseCase crearProducto,
        ObtenerProductoUseCase obtenerProducto,
        ActualizarProductoUseCase actualizarProducto,
        ListarProveedoresDeProductoUseCase listarProveedores,
        AsociarProveedorAProductoUseCase asociarProveedor,
        ActualizarProductoProveedorUseCase actualizarProductoProveedor)
    {
        _listarProductos = listarProductos;
        _crearProducto = crearProducto;
        _obtenerProducto = obtenerProducto;
        _actualizarProducto = actualizarProducto;
        _listarProveedores = listarProveedores;
        _asociarProveedor = asociarProveedor;
        _actualizarProductoProveedor = actualizarProductoProveedor;
    }

    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<ProductoResponse>>> Listar()
    {
        var resultado = _listarProductos.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<ProductoResponse>>.De(resultado));
    }

    // 05-api.md §13.2.
    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<ProductoResponse>> Obtener(int id)
    {
        var resultado = _obtenerProducto.Ejecutar(id);
        return Ok(ApiResponse<ProductoResponse>.De(resultado));
    }

    // 05-api.md §13.3.
    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<ProductoResponse>> Crear([FromBody] CrearProductoRequest request)
    {
        var resultado = _crearProducto.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ProductoResponse>.De(resultado));
    }

    // 05-api.md §13.4.
    [Authorize(Policy = "Permiso:PRODUCTO_EDITAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<ProductoResponse>> Actualizar(int id, [FromBody] ActualizarProductoRequest request)
    {
        var resultado = _actualizarProducto.Ejecutar(id, request);
        return Ok(ApiResponse<ProductoResponse>.De(resultado));
    }

    // TASK-019, 05-api.md §30.
    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet("{productoId:int}/proveedores")]
    public ActionResult<ApiResponse<IReadOnlyList<ProductoProveedorResponse>>> ListarProveedores(int productoId)
    {
        var resultado = _listarProveedores.Ejecutar(productoId);
        return Ok(ApiResponse<IReadOnlyList<ProductoProveedorResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost("{productoId:int}/proveedores")]
    public ActionResult<ApiResponse<ProductoProveedorResponse>> AsociarProveedor(
        int productoId, [FromBody] CrearProductoProveedorRequest request)
    {
        var resultado = _asociarProveedor.Ejecutar(productoId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ProductoProveedorResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PRODUCTO_EDITAR")]
    [HttpPut("{productoId:int}/proveedores/{relacionId:int}")]
    public ActionResult<ApiResponse<ProductoProveedorResponse>> ActualizarProveedor(
        int productoId, int relacionId, [FromBody] ActualizarProductoProveedorRequest request)
    {
        var resultado = _actualizarProductoProveedor.Ejecutar(productoId, relacionId, request);
        return Ok(ApiResponse<ProductoProveedorResponse>.De(resultado));
    }
}
