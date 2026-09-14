using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;

namespace AuropaqPedidos.Application.PedidosProveedor;

// D-08/RN-044 (01-reglas-negocio.md §11, cierre documental 2026-09-11): ENTREGADO -> CERRADO.
// Condición de negocio (cantidad pendiente de entrega = 0) la valida Domain (solo permite
// cerrar desde ENTREGADO, que ya implica cantidad pendiente = 0 — RN-034/RN-043). La existencia
// de Factura NO es requisito (RN-040): este caso de uso no consulta IFacturaRepository.
//
// D-11/RN-050: la autoría de quién cierra depende del mecanismo de autenticación real, que
// todavía no existe (Usuario, TASK-008, PENDIENTE) — no se agrega un parámetro de usuario
// provisional aquí (ver ADR-052).
public sealed class CerrarPedidoProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;

    public CerrarPedidoProveedorUseCase(IPedidoProveedorRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);

        pedido.Cerrar();

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
