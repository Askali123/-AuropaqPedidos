namespace AuropaqPedidos.Application.Entregas.Dtos;

// TASK-045, RN-034/04-base-datos.md §30: CantidadPendiente = CantidadPedida - SUM(CantidadEntregada).
public sealed record CantidadPendienteResponse(
    int DetallePedidoProveedorId,
    int ProductoId,
    int CantidadPedida,
    int CantidadEntregadaAcumulada,
    int CantidadPendiente);
