using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Facturas;

// TASK-046 (incremento MVP): agrega una línea a una Factura existente, referenciando un
// DetallePedidoProveedor por su Id (resuelto globalmente, sin asumir a qué pedido pertenece,
// para poder distinguir "no existe" de "pertenece a otro pedido"). La pertenencia al pedido de
// esta factura la valida Factura.AgregarDetalle (Domain). Antes de agregar la línea, calcula
// cuánto ya se había facturado en OTRAS facturas del mismo pedido (cruzando
// IFacturaRepository.ObtenerPorPedido) para que Domain pueda validar que la cantidad facturada
// acumulada no supere la cantidad pedida (decisión provisional de alcance MVP).
public sealed class AgregarDetalleFacturaUseCase
{
    private readonly IFacturaRepository _facturas;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDetalleFacturaUseCase(IFacturaRepository facturas, IPedidoProveedorRepository pedidos, IGeneradorDeIdentificadores ids)
    {
        _facturas = facturas;
        _pedidos = pedidos;
        _ids = ids;
    }

    public FacturaResponse Ejecutar(int facturaId, AgregarDetalleFacturaRequest request)
    {
        var factura = FacturaFinder.ObtenerOLanzar(_facturas, facturaId);

        var detallePedido = _pedidos.ObtenerDetallePorId(request.DetallePedidoProveedorId)
            ?? throw new RecursoNoEncontradoException($"El detalle de pedido {request.DetallePedidoProveedorId} no existe.");

        var facturasDelPedido = _facturas.ObtenerPorPedido(factura.PedidoProveedor.Id);
        var cantidadYaFacturada = FacturaFinder.CalcularCantidadYaFacturada(facturasDelPedido, detallePedido.Id);

        // Factura.AgregarDetalle valida internamente que detallePedido pertenezca al
        // PedidoProveedor de esta factura (ReglaDeNegocioException/422 si pertenece a otro pedido).
        factura.AgregarDetalle(_ids.Siguiente(), detallePedido, request.CantidadFacturada, request.PrecioUnitario, cantidadYaFacturada);

        _facturas.Guardar(factura);

        return FacturaMapper.AResponse(factura);
    }
}
