using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Consolidaciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// P2-1 (docs/2026-09-17-tareas.md): expone por HTTP el único caso de uso que existe para
// Consolidación (docs/05-api.md §31 documentaba 4 rutas conceptuales — GET listar, GET por id,
// POST crear, POST {id}/generar; solo "crear" tiene caso de uso real, `CrearConsolidacionUseCase`,
// TASK-036). Mismo criterio de minimalismo ya usado en FacturasController/PedidosProveedorController:
// no se inventan GET/generar sin un caso de uso que los respalde. El Controller solo traduce
// HTTP <-> caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20): no contiene reglas de negocio.
//
// Autorización: PEDIDO_CONSOLIDAR (ya catalogado desde RN-059, "Consolidar necesidades en pedidos
// a proveedor" — mismo permiso que la matriz de roles ya asigna a Compras/Administrador). Sin
// alcance por empresa: una Consolidación agrupa requisiciones aprobadas de cualquier empresa del
// mismo periodo (CLAUDE.md §25/§27, mismo criterio que Pedido/Entrega/Factura).
//
// GET listar/consultar agregados 2026-09-17 (I1-1,
// docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): usan PEDIDO_VER (RN-064/ADR-067) —
// Consolidación no tiene permiso de lectura propio, decisión explícita para no crear un permiso
// sin necesidad de negocio documentada.
[ApiController]
[Route("api/v1/consolidaciones")]
public sealed class ConsolidacionesController : ControllerBase
{
    private readonly CrearConsolidacionUseCase _crear;
    private readonly ListarConsolidacionesUseCase _listar;
    private readonly ObtenerConsolidacionUseCase _obtener;

    public ConsolidacionesController(
        CrearConsolidacionUseCase crear, ListarConsolidacionesUseCase listar, ObtenerConsolidacionUseCase obtener)
    {
        _crear = crear;
        _listar = listar;
        _obtener = obtener;
    }

    [Authorize(Policy = "Permiso:PEDIDO_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<ConsolidacionResponse>>> Listar([FromQuery] int? periodoId)
    {
        var resultado = _listar.Ejecutar(periodoId);
        return Ok(ApiResponse<IReadOnlyList<ConsolidacionResponse>>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PEDIDO_VER")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<ConsolidacionResponse>> Obtener(int id)
    {
        var resultado = _obtener.Ejecutar(id);
        return Ok(ApiResponse<ConsolidacionResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:PEDIDO_CONSOLIDAR")]
    [HttpPost]
    public ActionResult<ApiResponse<ConsolidacionResponse>> Crear([FromBody] CrearConsolidacionRequest request)
    {
        var resultado = _crear.Ejecutar(ObtenerUsuarioIdAutenticado(), DateTime.UtcNow, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ConsolidacionResponse>.De(resultado));
    }

    // Mismo criterio que RequisicionesController: para llegar aquí, [Authorize] ya garantizó un
    // JWT válido con el claim "sub".
    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
