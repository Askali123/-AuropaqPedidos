using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Api.Controllers.Dtos;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// Cierre documental 2026-09-11 (D-01 a D-13, 01-reglas-negocio.md §11/§15) + preparación de
// PedidosProveedorController. docs/05-api.md §32 marcaba estas rutas como conceptuales; con el
// ciclo de estados ya cerrado (D-03/RN-043) se exponen las acciones ya respaldadas por casos de
// uso existentes: crear, agregar detalle, agregar distribución, enviar, cerrar, cancelar, y
// (creación de Entrega por HTTP, 2026-09-11) crear entrega para el pedido — ruta anidada ya
// documentada en 05-api.md §33 ("POST /api/v1/pedidos-proveedor/{id}/entregas").
// Sin PUT ni "confirmar" (no existen casos de uso ni contrato definido para ellos — mismo
// criterio de minimalismo ya usado en FacturasController). El Controller solo traduce HTTP <->
// caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20): no contiene reglas de negocio.
//
// Autorización real agregada 2026-09-17 (RN-063/ADR-066, P1 de docs/2026-09-17-tareas.md):
// PEDIDO_CREAR (crear/agregar detalle/distribución), PEDIDO_ENVIAR, PEDIDO_CERRAR,
// PEDIDO_CANCELAR, y ENTREGA_REGISTRAR para la creación de Entrega anidada — mapeo completo en
// 06-seguridad.md §52. Sin alcance por empresa (CLAUDE.md §27).
//
// GET listar/consultar agregados 2026-09-17 (I1-2/I1-3,
// docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): PEDIDO_VER (listar/consultar pedido) y
// ENTREGA_VER (listar entregas de un pedido) — ya catalogados desde RN-059, sin necesitar
// ninguno nuevo.
[ApiController]
[Route("api/v1/pedidos-proveedor")]
public sealed class PedidosProveedorController : ControllerBase
{
    private readonly CrearPedidoProveedorUseCase _crear;
    private readonly ListarPedidosProveedorUseCase _listar;
    private readonly ObtenerPedidoProveedorUseCase _obtener;
    private readonly AgregarDetallePedidoProveedorUseCase _agregarDetalle;
    private readonly AgregarDistribucionPedidoUseCase _agregarDistribucion;
    private readonly EnviarPedidoProveedorUseCase _enviar;
    private readonly CerrarPedidoProveedorUseCase _cerrar;
    private readonly CancelarPedidoProveedorUseCase _cancelar;
    private readonly CrearEntregaUseCase _crearEntrega;
    private readonly ListarEntregasUseCase _listarEntregas;

    public PedidosProveedorController(
        CrearPedidoProveedorUseCase crear,
        ListarPedidosProveedorUseCase listar,
        ObtenerPedidoProveedorUseCase obtener,
        AgregarDetallePedidoProveedorUseCase agregarDetalle,
        AgregarDistribucionPedidoUseCase agregarDistribucion,
        EnviarPedidoProveedorUseCase enviar,
        CerrarPedidoProveedorUseCase cerrar,
        CancelarPedidoProveedorUseCase cancelar,
        CrearEntregaUseCase crearEntrega,
        ListarEntregasUseCase listarEntregas)
    {
        _crear = crear;
        _listar = listar;
        _obtener = obtener;
        _agregarDetalle = agregarDetalle;
        _agregarDistribucion = agregarDistribucion;
        _enviar = enviar;
        _cerrar = cerrar;
        _cancelar = cancelar;
        _crearEntrega = crearEntrega;
        _listarEntregas = listarEntregas;
    }

    // I1-2 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): listar/consultar agregados
    // para que el Frontend pueda elegir un pedido en vez de requerir un Id a mano.
    [Authorize(Policy = "Permiso:PEDIDO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<PedidoProveedorResponse>>> Listar([FromQuery] int? consolidacionId)
    {
        var resultado = _listar.Ejecutar(consolidacionId);
        return Ok(ApiResponse<IReadOnlyList<PedidoProveedorResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PEDIDO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Obtener(int id)
    {
        var resultado = _obtener.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // I1-3: mismo criterio de anidación ya usado para POST .../entregas (creación) — listar las
    // entregas de este pedido vive junto a su creación, no en EntregasController.
    [Authorize(Policy = "Permiso:ENTREGA_VER")]
    [HttpGet("{id:int}/entregas")]
    public ActionResult<ApiResponse<IReadOnlyList<EntregaResponse>>> ListarEntregas(int id)
    {
        var resultado = _listarEntregas.Ejecutar(id);
        return Ok(ApiResponse<IReadOnlyList<EntregaResponse>>.De(resultado));
    }

    // FechaPedido se toma del reloj del servidor al crear (mismo criterio ya usado para
    // Entrega.FechaEntrega/Factura.FechaFactura en sus respectivos Controllers).
    [Authorize(Policy = "Permiso:PEDIDO_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Crear([FromBody] CrearPedidoProveedorRequest request)
    {
        var resultado = _crear.Ejecutar(DateTime.UtcNow, request, ObtenerUsuarioIdAutenticado());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PEDIDO_CREAR")]
    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> AgregarDetalle(
        int id, [FromBody] AgregarDetallePedidoProveedorRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // Mismo criterio que RequisicionesController.AgregarDistribucion: sub-recurso "distribuciones"
    // bajo el detalle.
    [Authorize(Policy = "Permiso:PEDIDO_CREAR")]
    [HttpPost("{id:int}/detalles/{detalleId:int}/distribuciones")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> AgregarDistribucion(
        int id, int detalleId, [FromBody] AgregarDistribucionPedidoRequest request)
    {
        var resultado = _agregarDistribucion.Ejecutar(id, detalleId, request.SedeId, request.Cantidad);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-03/RN-043: BORRADOR -> ENVIADO.
    [Authorize(Policy = "Permiso:PEDIDO_ENVIAR")]
    [HttpPost("{id:int}/enviar")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Enviar(int id)
    {
        var resultado = _enviar.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-08/RN-044: ENTREGADO -> CERRADO. La factura no es requisito (RN-040).
    [Authorize(Policy = "Permiso:PEDIDO_CERRAR")]
    [HttpPost("{id:int}/cerrar")]
    public ActionResult<ApiResponse<PedidoProveedorResponse>> Cerrar(int id)
    {
        var resultado = _cerrar.Ejecutar(id);
        return Ok(ApiResponse<PedidoProveedorResponse>.De(resultado));
    }

    // D-03/RN-043: BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO -> CANCELADO.
    [Authorize(Policy = "Permiso:PEDIDO_CANCELAR")]
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
    [Authorize(Policy = "Permiso:ENTREGA_REGISTRAR")]
    [HttpPost("{id:int}/entregas")]
    public ActionResult<ApiResponse<EntregaResponse>> CrearEntrega(int id, [FromBody] CrearEntregaHttpRequest request)
    {
        var resultado = _crearEntrega.Ejecutar(
            DateTime.UtcNow, new CrearEntregaRequest(id, request.NumeroRemision, request.Observacion),
            ObtenerUsuarioIdAutenticado());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EntregaResponse>.De(resultado));
    }

    // Mismo criterio que RequisicionesController/ConsolidacionesController: para llegar aquí,
    // [Authorize] ya garantizó un JWT válido con el claim "sub".
    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
