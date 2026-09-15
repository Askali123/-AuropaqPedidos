using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-018, 05-api.md §29. El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40).
// RN-059/060 (punto 8, 2026-09-15): PROVEEDOR_VER/CREAR/EDITAR (06-seguridad.md §52/§53).
[ApiController]
[Route("api/v1/proveedores")]
public sealed class ProveedoresController : ControllerBase
{
    private readonly ListarProveedoresUseCase _listarProveedores;
    private readonly CrearProveedorUseCase _crearProveedor;
    private readonly ObtenerProveedorUseCase _obtenerProveedor;
    private readonly ActualizarProveedorUseCase _actualizarProveedor;
    private readonly ListarProductosDeProveedorUseCase _listarProductos;

    public ProveedoresController(
        ListarProveedoresUseCase listarProveedores,
        CrearProveedorUseCase crearProveedor,
        ObtenerProveedorUseCase obtenerProveedor,
        ActualizarProveedorUseCase actualizarProveedor,
        ListarProductosDeProveedorUseCase listarProductos)
    {
        _listarProveedores = listarProveedores;
        _crearProveedor = crearProveedor;
        _obtenerProveedor = obtenerProveedor;
        _actualizarProveedor = actualizarProveedor;
        _listarProductos = listarProductos;
    }

    [Authorize(Policy = "Permiso:PROVEEDOR_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<ProveedorResponse>>> Listar()
    {
        var resultado = _listarProveedores.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<ProveedorResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PROVEEDOR_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<ProveedorResponse>> Obtener(int id)
    {
        var resultado = _obtenerProveedor.Ejecutar(id);
        return Ok(ApiResponse<ProveedorResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PROVEEDOR_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<ProveedorResponse>> Crear([FromBody] CrearProveedorRequest request)
    {
        var resultado = _crearProveedor.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ProveedorResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PROVEEDOR_EDITAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<ProveedorResponse>> Actualizar(int id, [FromBody] ActualizarProveedorRequest request)
    {
        var resultado = _actualizarProveedor.Ejecutar(id, request);
        return Ok(ApiResponse<ProveedorResponse>.De(resultado));
    }

    // TASK-019, 05-api.md §30.
    [Authorize(Policy = "Permiso:PROVEEDOR_VER")]
    [HttpGet("{proveedorId:int}/productos")]
    public ActionResult<ApiResponse<IReadOnlyList<ProductoProveedorResponse>>> ListarProductos(int proveedorId)
    {
        var resultado = _listarProductos.Ejecutar(proveedorId);
        return Ok(ApiResponse<IReadOnlyList<ProductoProveedorResponse>>.De(resultado));
    }
}
