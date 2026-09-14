using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Api.Controllers.Dtos;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// Cierre técnico 2026-09-11 (D-04/RN-046, 01-reglas-negocio.md §12) + creación de Entrega por
// HTTP. docs/05-api.md §33 marca las rutas de Entrega bajo /api/v1/entregas/{id} como
// conceptuales; este controller expone las acciones ya respaldadas por un caso de uso existente
// y una regla de negocio cerrada: agregar detalle, agregar distribución, anular. La creación de
// la cabecera de Entrega vive en PedidosProveedorController (ruta anidada
// POST /api/v1/pedidos-proveedor/{id}/entregas, ya documentada). Sin GET ni otras acciones: no
// están respaldadas por un caso de uso HTTP en esta tarea (mismo criterio de minimalismo que
// FacturasController/PedidosProveedorController). El Controller solo traduce HTTP <-> caso de
// uso (CLAUDE.md §35/§40, 03-arquitectura.md §20).
[ApiController]
[Route("api/v1/entregas")]
public sealed class EntregasController : ControllerBase
{
    private readonly AgregarDetalleEntregaUseCase _agregarDetalle;
    private readonly AgregarDistribucionEntregaUseCase _agregarDistribucion;
    private readonly AnularEntregaUseCase _anular;

    public EntregasController(
        AgregarDetalleEntregaUseCase agregarDetalle, AgregarDistribucionEntregaUseCase agregarDistribucion, AnularEntregaUseCase anular)
    {
        _agregarDetalle = agregarDetalle;
        _agregarDistribucion = agregarDistribucion;
        _anular = anular;
    }

    // RN-034/RN-046, 04-base-datos.md §30 "Regla": la cantidad entregada acumulada no puede
    // superar CantidadPedida; AgregarDetalleEntregaUseCase también actualiza el estado del
    // PedidoProveedor (PARCIALMENTE_ENTREGADO/ENTREGADO) según corresponda.
    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<EntregaResponse>> AgregarDetalle(int id, [FromBody] AgregarDetalleEntregaRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EntregaResponse>.De(resultado));
    }

    // RN-035/ADR-019: distribución REAL de lo entregado, con snapshot histórico de sede. Mismo
    // criterio de sub-recurso que PedidosProveedorController.AgregarDistribucion.
    [HttpPost("{id:int}/detalles/{detalleId:int}/distribuciones")]
    public ActionResult<ApiResponse<EntregaResponse>> AgregarDistribucion(
        int id, int detalleId, [FromBody] AgregarDistribucionEntregaRequest request)
    {
        var resultado = _agregarDistribucion.Ejecutar(id, detalleId, request.SedeId, request.Cantidad);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EntregaResponse>.De(resultado));
    }

    // D-04/RN-046: REGISTRADA -> ANULADA.
    [HttpPost("{id:int}/anular")]
    public ActionResult<ApiResponse<EntregaResponse>> Anular(int id)
    {
        var resultado = _anular.Ejecutar(id);
        return Ok(ApiResponse<EntregaResponse>.De(resultado));
    }
}
