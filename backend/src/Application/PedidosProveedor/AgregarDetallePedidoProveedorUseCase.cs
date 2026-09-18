using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.PedidosProveedor;

// TASK-040: registra un producto (con su cantidad necesaria y cantidad pedida) dentro de un
// pedido ya creado, a partir de un DetalleConsolidacion de la misma consolidación del pedido.
//
// TASK-105 (docs/2026-09-18-auditoria-dominio-roles-frontend.md): además busca si existe una
// relación Producto-Proveedor activa para (producto del detalle, proveedor del pedido) y la
// pasa a Domain para que quede como fotografía en el detalle — sin exigirla: no todo producto
// tiene necesariamente un código registrado para ese proveedor todavía.
public sealed class AgregarDetallePedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IProductoProveedorRepository _productosProveedor;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDetallePedidoProveedorUseCase(
        IPedidoProveedorRepository pedidos, IProductoProveedorRepository productosProveedor, IGeneradorDeIdentificadores ids)
    {
        _pedidos = pedidos;
        _productosProveedor = productosProveedor;
        _ids = ids;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId, AgregarDetallePedidoProveedorRequest request)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);
        var detalleConsolidacion = PedidoProveedorFinder.ObtenerDetalleConsolidacionOLanzar(
            pedido.Consolidacion, request.DetalleConsolidacionId);

        var productoProveedor = _productosProveedor.ObtenerPorProducto(detalleConsolidacion.Producto.Id)
            .FirstOrDefault(pp => pp.Proveedor.Id == pedido.Proveedor.Id && pp.Activo);

        pedido.AgregarDetalle(_ids.Siguiente(), detalleConsolidacion, request.CantidadPedida, request.PrecioUnitario, productoProveedor);

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
