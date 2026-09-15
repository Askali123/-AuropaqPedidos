using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-017, 05-api.md §28. RN-059/060 (punto 8, 2026-09-15): PRODUCTO_SOLICITAR protege enviar
// una solicitud (rol Solicitante); PRODUCTO_VER/PRODUCTO_CREAR protegen resolverla (rol gestor de
// catálogo, sin alcance por empresa — un gestor central resuelve de cualquier empresa,
// 06-seguridad.md §12/§52/§53). El actor de negocio se deriva siempre del JWT.
[ApiController]
[Route("api/v1/solicitudes-producto")]
public sealed class SolicitudesProductoController : ControllerBase
{
    private readonly SolicitarProductoNoCatalogadoUseCase _solicitar;
    private readonly ListarSolicitudesPendientesUseCase _listarPendientes;
    private readonly HomologarProductoUseCase _homologar;
    private readonly CrearProductoDesdeSolicitudUseCase _crearProducto;
    private readonly RechazarSolicitudUseCase _rechazar;

    public SolicitudesProductoController(
        SolicitarProductoNoCatalogadoUseCase solicitar,
        ListarSolicitudesPendientesUseCase listarPendientes,
        HomologarProductoUseCase homologar,
        CrearProductoDesdeSolicitudUseCase crearProducto,
        RechazarSolicitudUseCase rechazar)
    {
        _solicitar = solicitar;
        _listarPendientes = listarPendientes;
        _homologar = homologar;
        _crearProducto = crearProducto;
        _rechazar = rechazar;
    }

    // 05-api.md §28.1.
    [Authorize(Policy = "Permiso:PRODUCTO_SOLICITAR")]
    [HttpPost]
    public ActionResult<ApiResponse<SolicitudProductoCatalogoResponse>> Solicitar([FromBody] SolicitarProductoRequest request)
    {
        var resultado = _solicitar.Ejecutar(ObtenerUsuarioIdAutenticado(), request, DateTime.UtcNow);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SolicitudProductoCatalogoResponse>.De(resultado));
    }

    // 05-api.md §28.2.
    [Authorize(Policy = "Permiso:PRODUCTO_VER")]
    [HttpGet("pendientes")]
    public ActionResult<ApiResponse<IReadOnlyList<SolicitudProductoCatalogoResponse>>> ListarPendientes()
    {
        var resultado = _listarPendientes.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<SolicitudProductoCatalogoResponse>>.De(resultado));
    }

    // 05-api.md §28.3.
    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost("{id:int}/homologar")]
    public ActionResult<ApiResponse<SolicitudProductoCatalogoResponse>> Homologar(int id, [FromBody] HomologarProductoRequest request)
    {
        var resultado = _homologar.Ejecutar(id, request, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow);
        return Ok(ApiResponse<SolicitudProductoCatalogoResponse>.De(resultado));
    }

    // 05-api.md §28.4.
    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost("{id:int}/crear-producto")]
    public ActionResult<ApiResponse<SolicitudProductoCatalogoResponse>> CrearProducto(int id, [FromBody] CrearProductoDesdeSolicitudRequest request)
    {
        var resultado = _crearProducto.Ejecutar(id, request, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow);
        return Ok(ApiResponse<SolicitudProductoCatalogoResponse>.De(resultado));
    }

    // 05-api.md §28.5.
    [Authorize(Policy = "Permiso:PRODUCTO_CREAR")]
    [HttpPost("{id:int}/rechazar")]
    public ActionResult<ApiResponse<SolicitudProductoCatalogoResponse>> Rechazar(int id, [FromBody] RechazarSolicitudRequest request)
    {
        var resultado = _rechazar.Ejecutar(id, request, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow);
        return Ok(ApiResponse<SolicitudProductoCatalogoResponse>.De(resultado));
    }

    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
