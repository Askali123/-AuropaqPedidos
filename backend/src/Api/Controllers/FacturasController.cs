using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Facturas.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-046 (incremento MVP + API). docs/05-api.md §34/§47 marca las rutas de Factura como
// conceptuales/posteriores al MVP; este bloque tiene autorización explícita para exponer
// únicamente el registro de cabecera, el agregado de líneas y (cierre técnico 2026-09-11,
// D-05/RN-047) la anulación — sin PUT/DELETE ni acciones de aprobar/validar/pagar/cerrar.
// El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20).
[ApiController]
[Route("api/v1/facturas")]
public sealed class FacturasController : ControllerBase
{
    private readonly RegistrarFacturaUseCase _registrar;
    private readonly AgregarDetalleFacturaUseCase _agregarDetalle;
    private readonly AnularFacturaUseCase _anular;

    public FacturasController(
        RegistrarFacturaUseCase registrar, AgregarDetalleFacturaUseCase agregarDetalle, AnularFacturaUseCase anular)
    {
        _registrar = registrar;
        _agregarDetalle = agregarDetalle;
        _anular = anular;
    }

    // FechaFactura se toma del reloj del servidor al registrar (mismo criterio ya usado para
    // PedidoProveedor.FechaPedido/Entrega.FechaEntrega en sus respectivos Controllers).
    [HttpPost]
    public ActionResult<ApiResponse<FacturaResponse>> Registrar([FromBody] RegistrarFacturaRequest request)
    {
        var resultado = _registrar.Ejecutar(DateTime.UtcNow, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FacturaResponse>.De(resultado));
    }

    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<FacturaResponse>> AgregarDetalle(int id, [FromBody] AgregarDetalleFacturaRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FacturaResponse>.De(resultado));
    }

    // D-05/RN-047: REGISTRADA -> ANULADA.
    [HttpPost("{id:int}/anular")]
    public ActionResult<ApiResponse<FacturaResponse>> Anular(int id)
    {
        var resultado = _anular.Ejecutar(id);
        return Ok(ApiResponse<FacturaResponse>.De(resultado));
    }
}
