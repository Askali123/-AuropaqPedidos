using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;

namespace AuropaqPedidos.Application.Facturas;

// Búsquedas/cálculos compartidos por los casos de uso de Factura. Mismo criterio que
// EntregaFinder/PedidoProveedorFinder: centraliza las excepciones "no encontrado" (404).
internal static class FacturaFinder
{
    public static Factura ObtenerOLanzar(IFacturaRepository repositorio, int facturaId)
    {
        return repositorio.ObtenerPorId(facturaId)
            ?? throw new RecursoNoEncontradoException($"La factura {facturaId} no existe.");
    }

    // D-10/RN-048, ampliado por B4 (cierre técnico 2026-09-11): suma CantidadFacturada de todas
    // las facturas REGISTRADAS (no ANULADAS) para un mismo DetallePedidoProveedor (de cualquier
    // Factura del pedido), necesaria para validar el tope de "no superar la cantidad pedida".
    // Una factura ANULADA deja de contar aquí: su cantidad queda libre para nuevas facturas.
    public static int CalcularCantidadYaFacturada(IReadOnlyList<Factura> facturasDelPedido, int detallePedidoProveedorId) =>
        facturasDelPedido
            .Where(f => f.Estado != FacturaEstado.Anulada)
            .SelectMany(f => f.Detalles)
            .Where(d => d.DetallePedidoOrigen.Id == detallePedidoProveedorId)
            .Sum(d => d.CantidadFacturada);
}
