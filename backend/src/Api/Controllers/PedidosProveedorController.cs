using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Api.Controllers.Dtos;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// Cierre documental 2026-09-11 (D-01 a D-13, 01-reglas-negocio.md §11/§15) + preparación de
// PedidosProveedorController. docs/05-api.md §32 marcaba estas rutas como conceptuales; con el
// ciclo de estados ya cerrado (D-03/RN-043) se exponen las acciones ya respaldadas por casos de
// uso existentes: crear, agregar detalle, agregar distribución, enviar, cerrar, cancelar, y
// (creación de Entrega por HTTP, 2026-09-11) crear entrega para el pedido — ruta anidada ya
// documentada en 05-api.md §33 ("POST /api/v1/pedidos-proveedor/{id}/entregas").
// Sin GET/PUT ni "confirmar" (no existen casos de uso ni contrato definido para ellos — mismo
// criterio de minimalismo ya usado en FacturasController). El Controller solo traduce HTTP <->
// caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20): no contiene reglas de negocio.
[ApiController]
[Route("api/v1/pedidos-proveedor")]
public sealed class PedidosProveedorController : ControllerBase
{
    private readonly CrearPedidoProveedorUseCase _crear;
    private readonly AgregarDetallePedidoProveedorUseCase _agregarDetalle;
    private readonly AgregarDistribucionPedidoUseCase _agregarDistribucion;
    private readonly EnviarPedidoProveedorUseCase _enviar;
    private readonly CerrarPedidoProveedorUseCase _cerrar;
    private readonly CancelarPedidoProveedorUseCase _cancelar;
    private readonly CrearEntregaUseCase _crearEntrega;

    public PedidosProveedorController(
        CrearPedidoProveedorUseCase crear,
        AgregarDetallePedidoProveedorUseCase agregarDetalle,
        AgregarDistribucionPedidoUseCase agregarDistribucion,
        EnviarPedidoProveedorUseCase enviar,
        CerrarPedidoProveedorUseCase cerrar,
        CancelarPedidoProveedorUseCase cancelar,
        CrearEntregaUseCase crearEntrega)
    {
        _crear = crear;
        _agregarDetalle = agregarDetalle;
        _agregarDistribucion = agregarDistribucion;
        _enviar = enviar;
        _cerrar = cerrar;
        _cancelar = cancelar;
        _crearEntrega = crearEntrega;
    }

    // FechaPedido se toma del reloj del servidor al crear (mismo criterio ya usado para
    // Entrega.FechaEntrega/Factura.FechaFactura en sus respectivos Controllers).
    [HttpPost]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Crear([FromBody] CrearPedidoProveedorRequest request)
    {
        var resultado = _crear.Ejecutar(DateTime.UtcNow, request, IdentidadOpcional.ObtenerUsuarioIdSiAutenticado(User));
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> AgregarDetalle(
        int id, [FromBody] AgregarDetallePedidoProveedorRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // Mismo criterio que RequisicionesController.AgregarDistribucion: sub-recurso "distribuciones"
    // bajo el detalle.
    [HttpPost("{id:int}/detalles/{detalleId:int}/distribuciones")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> AgregarDistribucion(
        int id, int detalleId, [FromBody] AgregarDistribucionPedidoRequest request)
    {
        var resultado = _agregarDistribucion.Ejecutar(id, detalleId, request.SedeId, request.Cantidad);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-03/RN-043: BORRADOR -> ENVIADO.
    [HttpPost("{id:int}/enviar")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Enviar(int id)
    {
        var resultado = _enviar.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-08/RN-044: ENTREGADO -> CERRADO. La factura no es requisito (RN-040).
    [HttpPost("{id:int}/cerrar")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Cerrar(int id)
    {
        var resultado = _cerrar.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-03/RN-043: BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO -> CANCELADO.
    [HttpPost("{id:int}/cancelar")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Cancelar(int id)
    {
        var resultado = _cancelar.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // docs/05-api.md §33. FechaEntrega se toma del reloj del servidor al crear (mismo criterio
    // ya usado para PedidoProveedor.FechaPedido/Factura.FechaFactura). El pedido de destino ya
    // viene en la ruta; CrearEntregaUseCase valida que exista y que esté ENVIADO/
    // PARCIALMENTE_ENTREGADO (RN-046, Entrega.ctor).
    [HttpPost("{id:int}/entregas")]
    public ActionResult<ApiResponse<EntregaResponse>> CrearEntrega(int id, [FromBody] CrearEntregaHttpRequest request)
    {
        var resultado = _crearEntrega.Ejecutar(
            DateTime.UtcNow, new CrearEntregaRequest(id, request.NumeroRemision, request.Observacion),
            IdentidadOpcional.ObtenerUsuarioIdSiAutenticado(User));
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EntregaResponse>.De(resultado));
    }
}
