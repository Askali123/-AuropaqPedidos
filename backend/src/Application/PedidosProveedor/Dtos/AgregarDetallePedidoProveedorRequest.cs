namespace AuropaqPedidos.Application.PedidosProveedor.Dtos;

// CantidadPedida se exige explícita (nunca se copia automáticamente de CantidadNecesaria):
// RN-031 permite que difieran.
public sealed record AgregarDetallePedidoProveedorRequest(int DetalleConsolidacionId, int CantidadPedida, decimal? PrecioUnitario = null);
