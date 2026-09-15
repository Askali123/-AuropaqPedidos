using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.PedidosProveedor;

// D-08/RN-044 (01-reglas-negocio.md §11, cierre documental 2026-09-11): ENTREGADO -> CERRADO.
// Condición de negocio (cantidad pendiente de entrega = 0) la valida Domain (solo permite
// cerrar desde ENTREGADO, que ya implica cantidad pendiente = 0 — RN-034/RN-043). La existencia
// de Factura NO es requisito (RN-040): este caso de uso no consulta IFacturaRepository.
//
// D-11/RN-050: "acción explícita de un usuario autorizado" (RN-044) sigue sin implementarse
// aquí — a diferencia de 2026-09-11, ya existe Usuario/autenticación real (TASK-008/048), pero
// extender [Authorize]/alcance a Fase 6-9 quedó explícitamente pospuesto (decisión del usuario,
// 2026-09-15, ver progreso.md) para no mezclarlo con TASK-055/056 (logging/auditoría).
public sealed class CerrarPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly ILogger<CerrarPedidoProveedorUseCase> _logger;

    public CerrarPedidoProveedorUseCase(IPedidoProveedorRepository pedidos, ILogger<CerrarPedidoProveedorUseCase> logger)
    {
        _pedidos = pedidos;
        _logger = logger;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);

        pedido.Cerrar();

        _pedidos.Guardar(pedido);

        _logger.LogInformation("PedidoProveedor {PedidoId} cerrado", pedidoId);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
