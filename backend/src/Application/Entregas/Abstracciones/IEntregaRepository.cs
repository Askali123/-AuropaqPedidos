using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Entregas.Abstracciones;

public interface IEntregaRepository
{
    Entrega? ObtenerPorId(int id);

    // RN-033/04-base-datos.md §30: la cantidad entregada acumulada de un DetallePedidoProveedor
    // se calcula sumando CantidadEntregada de todas sus entregas, no solo de una — se necesita
    // poder consultar todas las entregas de un pedido para ese cálculo (TASK-043/045).
    IReadOnlyList<Entrega> ObtenerPorPedido(int pedidoProveedorId);

    void Guardar(Entrega entrega);
}
