using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-006/007 (05-api.md §11/§12): CRUD real de Empresa y creación de Sede. El Controller solo
// traduce HTTP <-> caso de uso (CLAUDE.md §35/§40). RN-059/060 (punto 8, 2026-09-15):
// ORGANIZACION_VER/ADMINISTRAR, sin alcance por empresa (administra empresas, 06-seguridad.md
// §52/§53).
[ApiController]
[Route("api/v1/empresas")]
public sealed class EmpresasController : ControllerBase
{
    private readonly ListarEmpresasUseCase _listarEmpresas;
    private readonly ListarSedesPorEmpresaUseCase _listarSedes;
    private readonly CrearEmpresaUseCase _crearEmpresa;
    private readonly ObtenerEmpresaUseCase _obtenerEmpresa;
    private readonly ActualizarEmpresaUseCase _actualizarEmpresa;
    private readonly CrearSedeUseCase _crearSede;

    public EmpresasController(
        ListarEmpresasUseCase listarEmpresas,
        ListarSedesPorEmpresaUseCase listarSedes,
        CrearEmpresaUseCase crearEmpresa,
        ObtenerEmpresaUseCase obtenerEmpresa,
        ActualizarEmpresaUseCase actualizarEmpresa,
        CrearSedeUseCase crearSede)
    {
        _listarEmpresas = listarEmpresas;
        _listarSedes = listarSedes;
        _crearEmpresa = crearEmpresa;
        _obtenerEmpresa = obtenerEmpresa;
        _actualizarEmpresa = actualizarEmpresa;
        _crearSede = crearSede;
    }

    [Authorize(Policy = "Permiso:ORGANIZACION_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<EmpresaResponse>>> Listar()
    {
        var resultado = _listarEmpresas.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<EmpresaResponse>>.De(resultado));
    }

    // 05-api.md §11.2.
    [Authorize(Policy = "Permiso:ORGANIZACION_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<EmpresaResponse>> Obtener(int id)
    {
        var resultado = _obtenerEmpresa.Ejecutar(id);
        return Ok(ApiResponse<EmpresaResponse>.De(resultado));
    }

    // 05-api.md §11.3.
    [Authorize(Policy = "Permiso:ORGANIZACION_ADMINISTRAR")]
    [HttpPost]
    public ActionResult<ApiResponse<EmpresaResponse>> Crear([FromBody] CrearEmpresaRequest request)
    {
        var resultado = _crearEmpresa.Ejecutar(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EmpresaResponse>.De(resultado));
    }

    // 05-api.md §11.4.
    [Authorize(Policy = "Permiso:ORGANIZACION_ADMINISTRAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<EmpresaResponse>> Actualizar(int id, [FromBody] ActualizarEmpresaRequest request)
    {
        var resultado = _actualizarEmpresa.Ejecutar(id, request);
        return Ok(ApiResponse<EmpresaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:ORGANIZACION_VER")]
    [HttpGet("{empresaId:int}/sedes")]
    public ActionResult<ApiResponse<IReadOnlyList<SedeResponse>>> ListarSedes(int empresaId)
    {
        var resultado = _listarSedes.Ejecutar(empresaId);
        return Ok(ApiResponse<IReadOnlyList<SedeResponse>>.De(resultado));
    }

    // 05-api.md §12.2.
    [Authorize(Policy = "Permiso:ORGANIZACION_ADMINISTRAR")]
    [HttpPost("{empresaId:int}/sedes")]
    public ActionResult<ApiResponse<SedeResponse>> CrearSede(int empresaId, [FromBody] CrearSedeRequest request)
    {
        var resultado = _crearSede.Ejecutar(empresaId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SedeResponse>.De(resultado));
    }
}
