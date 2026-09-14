namespace AuropaqPedidos.Application.PedidosProveedor.Dtos;

// ProveedorId y NumeroPedido se reciben explícitos del llamador (D-01/D-09,
// 01-reglas-negocio.md §11/§15, cierre documental 2026-09-11): no hay selección automática de
// proveedor ni generación automática de número de pedido. Estado ya no se recibe: un pedido
// siempre se crea en BORRADOR (D-03/RN-043).
public sealed record CrearPedidoProveedorRequest(
    int ConsolidacionId,
    int ProveedorId,
    string NumeroPedido,
    DateTime? FechaEntregaEstimada = null,
    string? Observacion = null);
