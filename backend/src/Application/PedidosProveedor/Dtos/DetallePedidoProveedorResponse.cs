namespace AuropaqPedidos.Application.PedidosProveedor.Dtos;

// DetalleConsolidacionId no es una columna real (04-base-datos.md §27 no la incluye); se
// calcula al mapear, buscando en la Consolidacion del pedido el DetalleConsolidacion del mismo
// Producto (ver PedidoProveedorMapper), solo para exponer la trazabilidad en la respuesta.
public sealed record DetallePedidoProveedorResponse(
    int Id,
    int ProductoId,
    int? DetalleConsolidacionId,
    int CantidadNecesaria,
    int CantidadPedida,
    decimal? PrecioUnitario,
    IReadOnlyList<DistribucionPedidoResponse> Distribuciones);
