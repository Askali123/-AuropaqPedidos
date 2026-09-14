using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Facturas.Abstracciones;

public interface IFacturaRepository
{
    Factura? ObtenerPorId(int id);

    // Decisión provisional de alcance MVP/04-base-datos.md §30 (adaptado): la cantidad
    // facturada acumulada de un DetallePedidoProveedor se calcula sumando CantidadFacturada de
    // todas sus facturas, no solo de una — se necesita poder consultar todas las facturas de un
    // pedido para ese cálculo.
    IReadOnlyList<Factura> ObtenerPorPedido(int pedidoProveedorId);

    // D-09/RN-049 (01-reglas-negocio.md §15, cierre documental 2026-09-11): NumeroFactura debe
    // ser único dentro del proveedor.
    bool ExisteNumeroFacturaParaProveedor(int proveedorId, string numeroFactura);

    void Guardar(Factura factura);
}
