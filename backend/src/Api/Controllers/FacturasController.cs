using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Facturas.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-046 (incremento MVP + API). docs/05-api.md §34/§47 marca las rutas de Factura como
// conceptuales/posteriores al MVP; este bloque tiene autorización explícita para exponer
// únicamente el registro de cabecera, el agregado de líneas y (cierre técnico 2026-09-11,
// D-05/RN-047) la anulación — sin PUT/DELETE ni acciones de aprobar/validar/pagar/cerrar.
// El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20).
//
// Autorización real agregada 2026-09-17 (RN-063/ADR-066, P1 de docs/2026-09-17-tareas.md):
// FACTURA_REGISTRAR (registrar/agregar detalle) y FACTURA_ANULAR — mapeo completo en
// 06-seguridad.md §52. Sin alcance por empresa (CLAUDE.md §27).
//
// GET listar/consultar agregados 2026-09-17 (I1-4,
// docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): FACTURA_VER, ya catalogado desde
// RN-059. Listar exige pedidoProveedorId (reutiliza IFacturaRepository.ObtenerPorPedido, ya
// existente para el cálculo de CantidadFacturada acumulada) — sin filtro por proveedor: nadie lo
// pidió y no hay caso de uso que lo respalde.
[ApiController]
[Route("api/v1/facturas")]
public sealed class FacturasController : ControllerBase
{
    private readonly RegistrarFacturaUseCase _registrar;
    private readonly ListarFacturasUseCase _listar;
    private readonly ObtenerFacturaUseCase _obtener;
    private readonly AgregarDetalleFacturaUseCase _agregarDetalle;
    private readonly AnularFacturaUseCase _anular;

    public FacturasController(
        RegistrarFacturaUseCase registrar, ListarFacturasUseCase listar, ObtenerFacturaUseCase obtener,
        AgregarDetalleFacturaUseCase agregarDetalle, AnularFacturaUseCase anular)
    {
        _registrar = registrar;
        _listar = listar;
        _obtener = obtener;
        _agregarDetalle = agregarDetalle;
        _anular = anular;
    }

    [Authorize(Policy = "Permiso:FACTURA_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<FacturaResponse>>> Listar([FromQuery] int pedidoProveedorId)
    {
        var resultado = _listar.Ejecutar(pedidoProveedorId);
        return Ok(ApiResponse<IReadOnlyList<FacturaResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:FACTURA_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<FacturaResponse>> Obtener(int id)
    {
        var resultado = _obtener.Ejecutar(id);
        return Ok(ApiResponse<FacturaResponse>.De(resultado));
    }

    // FechaFactura se toma del reloj del servidor al registrar (mismo criterio ya usado para
    // PedidoProveedor.FechaPedido/Entrega.FechaEntrega en sus respectivos Controllers).
    [Authorize(Policy = "Permiso:FACTURA_REGISTRAR")]
    [HttpPost]
    public ActionResult<ApiResponse<FacturaResponse>> Registrar([FromBody] RegistrarFacturaRequest request)
    {
        var resultado = _registrar.Ejecutar(DateTime.UtcNow, request, ObtenerUsuarioIdAutenticado());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FacturaResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:FACTURA_REGISTRAR")]
    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<FacturaResponse>> AgregarDetalle(int id, [FromBody] AgregarDetalleFacturaRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FacturaResponse>.De(resultado));
    }

    // D-05/RN-047: REGISTRADA -> ANULADA.
    [Authorize(Policy = "Permiso:FACTURA_ANULAR")]
    [HttpPost("{id:int}/anular")]
    public ActionResult<ApiResponse<FacturaResponse>> Anular(int id)
    {
        var resultado = _anular.Ejecutar(id);
        return Ok(ApiResponse<FacturaResponse>.De(resultado));
    }

    // Mismo criterio que RequisicionesController/ConsolidacionesController/
    // PedidosProveedorController: para llegar aquí, [Authorize] ya garantizó un JWT válido con
    // el claim "sub".
    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
