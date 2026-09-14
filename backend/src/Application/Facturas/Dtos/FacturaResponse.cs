namespace AuropaqPedidos.Application.Facturas.Dtos;

public sealed record FacturaResponse(
    int Id,
    int ProveedorId,
    int PedidoProveedorId,
    string NumeroFactura,
    DateTime FechaFactura,
    decimal Subtotal,
    decimal Impuestos,
    decimal Total,
    string Estado,
    string? Observacion,
    IReadOnlyList<DetalleFacturaResponse> Detalles);
