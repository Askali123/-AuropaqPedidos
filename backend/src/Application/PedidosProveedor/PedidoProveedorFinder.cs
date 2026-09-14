using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.PedidosProveedor;

// Búsquedas compartidas por los casos de uso de PedidoProveedor. Mismo criterio que
// RequisicionFinder: centraliza las excepciones "no encontrado" (404).
internal static class PedidoProveedorFinder
{
    public static PedidoProveedor ObtenerOLanzar(IPedidoProveedorRepository repositorio, int pedidoId)
    {
        return repositorio.ObtenerPorId(pedidoId)
            ?? throw new RecursoNoEncontradoException($"El pedido a proveedor {pedidoId} no existe.");
    }

    public static DetalleConsolidacion ObtenerDetalleConsolidacionOLanzar(Consolidacion consolidacion, int detalleConsolidacionId)
    {
        return consolidacion.Detalles.FirstOrDefault(d => d.Id == detalleConsolidacionId)
            ?? throw new RecursoNoEncontradoException(
                $"El detalle de consolidación {detalleConsolidacionId} no existe en la consolidación {consolidacion.Id}.");
    }

    public static DetallePedidoProveedor ObtenerDetalleOLanzar(PedidoProveedor pedido, int detalleId)
    {
        return pedido.Detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new RecursoNoEncontradoException(
                $"El detalle {detalleId} no existe en el pedido {pedido.Id}.");
    }
}
