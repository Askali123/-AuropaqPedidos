using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.PedidosProveedor;

// TASK-040: registra un producto (con su cantidad necesaria y cantidad pedida) dentro de un
// pedido ya creado, a partir de un DetalleConsolidacion de la misma consolidación del pedido.
public sealed class AgregarDetallePedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDetallePedidoProveedorUseCase(IPedidoProveedorRepository pedidos, IGeneradorDeIdentificadores ids)
    {
        _pedidos = pedidos;
        _ids = ids;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId, AgregarDetallePedidoProveedorRequest request)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);
        var detalleConsolidacion = PedidoProveedorFinder.ObtenerDetalleConsolidacionOLanzar(
            pedido.Consolidacion, request.DetalleConsolidacionId);

        pedido.AgregarDetalle(_ids.Siguiente(), detalleConsolidacion, request.CantidadPedida, request.PrecioUnitario);

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
