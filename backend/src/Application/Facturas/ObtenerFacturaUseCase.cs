using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;

namespace AuropaqPedidos.Application.Facturas;

// I1-4 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): consultar el detalle de una
// factura (detalles, subtotal/total, estado).
public sealed class ObtenerFacturaUseCase
{
    private readonly IFacturaRepository _facturas;

    public ObtenerFacturaUseCase(IFacturaRepository facturas)
    {
        _facturas = facturas;
    }

    public FacturaResponse Ejecutar(int id)
    {
        var factura = _facturas.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La factura {id} no existe.");

        return FacturaMapper.AResponse(factura);
    }
}
