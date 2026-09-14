using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Entregas;

// TASK-043, RN-033/RN-034, 04-base-datos.md §30 "Regla": registra cuánto se entregó de un
// producto del pedido. Antes de agregarlo, calcula cuánto ya se había entregado en OTRAS
// entregas del mismo pedido (cruzando IEntregaRepository.ObtenerPorPedido) para que Domain
// pueda validar que la cantidad entregada acumulada no supere la cantidad pedida.
//
// Cierre documental 2026-09-11 (sección 9 de la tarea, D-03/RN-043): después de registrar el
// detalle, recalcula si queda cantidad pendiente en CUALQUIER DetallePedidoProveedor del pedido
// (no solo el de este detalle) y actualiza PedidoProveedor.Estado a PARCIALMENTE_ENTREGADO o
// ENTREGADO. El cierre (ENTREGADO -> CERRADO) sigue siendo una acción explícita aparte
// (CerrarPedidoProveedorUseCase, D-08) — nunca automática.
public sealed class AgregarDetalleEntregaUseCase
{
    private readonly IEntregaRepository _entregas;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly ITransaccionDeEntrega _transaccion;

    public AgregarDetalleEntregaUseCase(
        IEntregaRepository entregas,
        IPedidoProveedorRepository pedidos,
        IGeneradorDeIdentificadores ids,
        ITransaccionDeEntrega transaccion)
    {
        _entregas = entregas;
        _pedidos = pedidos;
        _ids = ids;
        _transaccion = transaccion;
    }

    public EntregaResponse Ejecutar(int entregaId, AgregarDetalleEntregaRequest request)
    {
        var entrega = EntregaFinder.ObtenerOLanzar(_entregas, entregaId);
        var detallePedido = PedidoProveedorFinder.ObtenerDetalleOLanzar(entrega.PedidoProveedor, request.DetallePedidoProveedorId);

        var entregasDelPedido = _entregas.ObtenerPorPedido(entrega.PedidoProveedor.Id);
        var cantidadYaEntregada = EntregaFinder.CalcularCantidadYaEntregada(entregasDelPedido, detallePedido.Id);

        // Auditoría (problema crítico de atomicidad): modificar Entrega y PedidoProveedor.Estado
        // debe confirmarse junto o no confirmarse nada — de lo contrario un fallo entre los dos
        // Guardar() deja el pedido con un Estado que ya no corresponde a sus entregas reales.
        _transaccion.Ejecutar(() =>
        {
            entrega.AgregarDetalle(_ids.Siguiente(), detallePedido, request.CantidadEntregada, cantidadYaEntregada);

            _entregas.Guardar(entrega);

            // entregasDelPedido contiene la misma instancia de "entrega" ya mutada (mismo
            // criterio de identidad que el resto de Application: EF Core la resuelve por change
            // tracking dentro del mismo DbContext; los fakes de prueba, por referencia de
            // objeto), así que ya refleja la cantidad recién agregada sin necesidad de volver a
            // consultar.
            var pedido = entrega.PedidoProveedor;
            var (hayAlgunaCantidadEntregada, quedaCantidadPendiente) =
                EntregaFinder.CalcularEstadoAgregadoDeEntregas(pedido, entregasDelPedido);

            pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada, quedaCantidadPendiente);
            _pedidos.Guardar(pedido);
        });

        return EntregaMapper.AResponse(entrega);
    }
}
