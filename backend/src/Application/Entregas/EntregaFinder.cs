using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;

namespace AuropaqPedidos.Application.Entregas;

// Búsquedas compartidas por los casos de uso de Entrega. Mismo criterio que
// RequisicionFinder/PedidoProveedorFinder: centraliza las excepciones "no encontrado" (404).
internal static class EntregaFinder
{
    public static Entrega ObtenerOLanzar(IEntregaRepository repositorio, int entregaId)
    {
        return repositorio.ObtenerPorId(entregaId)
            ?? throw new RecursoNoEncontradoException($"La entrega {entregaId} no existe.");
    }

    public static DetalleEntrega ObtenerDetalleOLanzar(Entrega entrega, int detalleId)
    {
        return entrega.Detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new RecursoNoEncontradoException(
                $"El detalle {detalleId} no existe en la entrega {entrega.Id}.");
    }

    // RN-033/04-base-datos.md §30: suma CantidadEntregada de todas las entregas REGISTRADAS
    // (no ANULADAS) para un mismo DetallePedidoProveedor (de cualquier Entrega del pedido),
    // necesaria para validar el tope de "no superar la cantidad pedida" y para calcular la
    // cantidad pendiente. Cierre técnico 2026-09-11 (B1): una entrega ANULADA deja de contar
    // aquí, tanto para el tope de nuevas entregas como para el estado del PedidoProveedor.
    public static int CalcularCantidadYaEntregada(IReadOnlyList<Entrega> entregasDelPedido, int detallePedidoProveedorId) =>
        entregasDelPedido
            .Where(e => e.Estado != EntregaEstado.Anulada)
            .SelectMany(e => e.Detalles)
            .Where(d => d.DetallePedidoOrigen.Id == detallePedidoProveedorId)
            .Sum(d => d.CantidadEntregada);

    // Sección 9 (D-03/RN-043) + B1 (cierre técnico 2026-09-11): recorre TODOS los
    // DetallePedidoProveedor del pedido (no solo el de un detalle recién agregado/anulado) para
    // determinar los dos booleanos que necesita PedidoProveedor.ActualizarEstadoPorEntregas.
    // Compartido por AgregarDetalleEntregaUseCase y AnularEntregaUseCase para no duplicar la
    // lógica de recálculo.
    public static (bool HayAlgunaCantidadEntregada, bool QuedaCantidadPendiente) CalcularEstadoAgregadoDeEntregas(
        PedidoProveedor pedido, IReadOnlyList<Entrega> entregasDelPedido)
    {
        var hayAlgunaCantidadEntregada = false;
        var quedaCantidadPendiente = false;

        foreach (var detalle in pedido.Detalles)
        {
            var cantidadEntregada = CalcularCantidadYaEntregada(entregasDelPedido, detalle.Id);
            if (cantidadEntregada > 0)
                hayAlgunaCantidadEntregada = true;
            if (cantidadEntregada < detalle.CantidadPedida)
                quedaCantidadPendiente = true;
        }

        return (hayAlgunaCantidadEntregada, quedaCantidadPendiente);
    }
}
