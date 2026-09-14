using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

public interface IPedidoProveedorRepository
{
    PedidoProveedor? ObtenerPorId(int id);

    // Necesario para Facturación (TASK-046, incremento MVP): validar si un
    // DetallePedidoProveedor pertenece al PedidoProveedor de una Factura requiere poder
    // resolver el detalle por su Id sin conocer de antemano a qué pedido pertenece (a diferencia
    // de PedidoProveedorFinder.ObtenerDetalleOLanzar, que busca dentro de un pedido ya conocido).
    // Esto permite distinguir "detalle no existe" (404) de "detalle pertenece a otro pedido" (422).
    DetallePedidoProveedor? ObtenerDetallePorId(int detalleId);

    // D-09/RN-049 (01-reglas-negocio.md §15, cierre documental 2026-09-11): NumeroPedido debe
    // ser único dentro del proveedor.
    bool ExisteNumeroPedidoParaProveedor(int proveedorId, string numeroPedido);

    void Guardar(PedidoProveedor pedido);
}
