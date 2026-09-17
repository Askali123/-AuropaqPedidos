using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;

namespace AuropaqPedidos.Application.Facturas;

// I1-4 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): permite al Frontend listar las
// facturas de un pedido (reutiliza IFacturaRepository.ObtenerPorPedido, ya existente para el
// cálculo de CantidadFacturada acumulada) en vez de requerir un Id de Factura a mano.
public sealed class ListarFacturasUseCase
{
    private readonly IFacturaRepository _facturas;

    public ListarFacturasUseCase(IFacturaRepository facturas)
    {
        _facturas = facturas;
    }

    public IReadOnlyList<FacturaResponse> Ejecutar(int pedidoProveedorId) =>
        _facturas.ObtenerPorPedido(pedidoProveedorId)
            .Select(FacturaMapper.AResponse)
            .ToList();
}
