using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Facturas;

// D-05/RN-047 (01-reglas-negocio.md §13, cierre documental 2026-09-11): REGISTRADA -> ANULADA.
// Sin estados contables (RN-038). No recalcula la cantidad ya facturada acumulada de otras
// facturas del pedido: ninguna decisión de negocio documentada dice que una factura anulada
// deba liberar cupo contra CantidadPedida (evitar inventar una regla no cerrada).
public sealed class AnularFacturaUseCase
{
    private readonly IFacturaRepository _facturas;
    private readonly ILogger<AnularFacturaUseCase> _logger;

    public AnularFacturaUseCase(IFacturaRepository facturas, ILogger<AnularFacturaUseCase> logger)
    {
        _facturas = facturas;
        _logger = logger;
    }

    public FacturaResponse Ejecutar(int facturaId)
    {
        var factura = FacturaFinder.ObtenerOLanzar(_facturas, facturaId);

        factura.Anular();

        _facturas.Guardar(factura);

        _logger.LogInformation("Factura {FacturaId} anulada", facturaId);

        return FacturaMapper.AResponse(factura);
    }
}
