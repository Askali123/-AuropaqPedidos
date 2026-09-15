using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.PedidosProveedor;

// D-03/RN-043 (01-reglas-negocio.md §11, cierre documental 2026-09-11):
// BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO -> CANCELADO.
public sealed class CancelarPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly ILogger<CancelarPedidoProveedorUseCase> _logger;

    public CancelarPedidoProveedorUseCase(IPedidoProveedorRepository pedidos, ILogger<CancelarPedidoProveedorUseCase> logger)
    {
        _pedidos = pedidos;
        _logger = logger;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);

        pedido.Cancelar();

        _pedidos.Guardar(pedido);

        _logger.LogInformation("PedidoProveedor {PedidoId} cancelado", pedidoId);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
