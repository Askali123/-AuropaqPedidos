using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Facturas;

// TASK-046 (incremento MVP): registra la cabecera de una Factura asociada a un Proveedor y a un
// PedidoProveedor existentes (decisión provisional de alcance MVP: PedidoProveedor 1 ─── N
// Facturas). La coherencia proveedor↔pedido la valida el constructor de Factura (Domain).
//
// "Proveedor no activo" se valida aquí (no en Domain), mismo precedente ya usado en
// CrearPedidoProveedorUseCase: Domain solo exige que el proveedor no sea null.
public sealed class RegistrarFacturaUseCase
{
    private readonly IFacturaRepository _facturas;
    private readonly IProveedorRepository _proveedores;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;

    public RegistrarFacturaUseCase(
        IFacturaRepository facturas, IProveedorRepository proveedores, IPedidoProveedorRepository pedidos, IGeneradorDeIdentificadores ids)
    {
        _facturas = facturas;
        _proveedores = proveedores;
        _pedidos = pedidos;
        _ids = ids;
    }

    public FacturaResponse Ejecutar(DateTime fechaFactura, RegistrarFacturaRequest request)
    {
        var proveedor = _proveedores.ObtenerPorId(request.ProveedorId)
            ?? throw new RecursoNoEncontradoException("El proveedor indicado no existe.");

        if (!proveedor.Activo)
            throw new ReglaDeNegocioException("El proveedor no está activo.");

        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, request.PedidoProveedorId);

        // D-09/RN-049: NumeroFactura único dentro del proveedor.
        if (_facturas.ExisteNumeroFacturaParaProveedor(proveedor.Id, request.NumeroFactura))
            throw new ReglaDeNegocioException(
                $"Ya existe una factura con el número '{request.NumeroFactura}' para este proveedor.");

        var factura = new Factura(
            id: _ids.Siguiente(),
            proveedor: proveedor,
            pedidoProveedor: pedido,
            numeroFactura: request.NumeroFactura,
            fechaFactura: fechaFactura,
            impuestos: request.Impuestos,
            observacion: request.Observacion);

        _facturas.Guardar(factura);

        return FacturaMapper.AResponse(factura);
    }
}
