namespace AuropaqPedidos.Application.Facturas.Dtos;

// D-06/RN-039 (cierre documental 2026-09-11): la factura pertenece a UN PedidoProveedor
// (PedidoProveedorId obligatorio). Subtotal/Total NO se reciben: se calculan a partir de las
// líneas agregadas con AgregarDetalleFactura. Impuestos se registra tal como lo indica la
// factura física del proveedor (sin cálculo de tasa). Estado ya no se recibe: una factura
// siempre se crea REGISTRADA (D-05/RN-047).
public sealed record RegistrarFacturaRequest(
    int ProveedorId,
    int PedidoProveedorId,
    string NumeroFactura,
    decimal Impuestos,
    string? Observacion = null);
