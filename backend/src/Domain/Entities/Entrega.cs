using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-033, 02-dominio.md §24, 04-base-datos.md §29: entrega física realizada por el proveedor
// para un PedidoProveedor. Un pedido puede tener varias entregas (parciales). Es el agregado
// raíz de DetalleEntrega/DistribucionEntrega, mismo criterio que Requisicion/Consolidacion/
// PedidoProveedor.
//
// NumeroRemision se recibe explícito del llamador (mismo criterio ya usado para
// PedidoProveedor.NumeroPedido — Pendiente 9 de 01-reglas-negocio.md §15: no hay regla
// documentada de autogeneración). Cierre documental 2026-09-11 (D-04/RN-046): Estado es un enum
// (REGISTRADA/ANULADA), separado de las condiciones de recepción detalladas (Pendiente 4, que
// sigue sin definir). Una entrega solo puede registrarse contra un pedido ENVIADO o
// PARCIALMENTE_ENTREGADO (D-03/RN-043): no tiene sentido recibir mercancía de un pedido que
// todavía no se envió, ni de uno cancelado o cerrado.
//
// UsuarioCreacionId agregado 2026-09-17 (P2-2, docs/2026-09-17-tareas.md) — mismo motivo que
// PedidoProveedor.UsuarioCreacionId.
public sealed class Entrega
{
    public int Id { get; }
    public PedidoProveedor PedidoProveedor { get; }
    public int UsuarioCreacionId { get; }
    public DateTime FechaEntrega { get; }
    public string NumeroRemision { get; }
    public EntregaEstado Estado { get; private set; }
    public string? Observacion { get; }

    private readonly List<DetalleEntrega> _detalles = new();
    public IReadOnlyList<DetalleEntrega> Detalles => _detalles;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Entrega()
    {
    }
#pragma warning restore CS8618

    public Entrega(int id, PedidoProveedor pedidoProveedor, int usuarioCreacionId, DateTime fechaEntrega, string numeroRemision, string? observacion = null)
    {
        if (pedidoProveedor is null)
            throw new ReglaDeNegocioException("Una entrega debe pertenecer a un pedido.");

        if (pedidoProveedor.Estado is not (PedidoProveedorEstado.Enviado or PedidoProveedorEstado.ParcialmenteEntregado))
            throw new ReglaDeNegocioException(
                "Solo puede registrarse una entrega para un pedido ENVIADO o PARCIALMENTE_ENTREGADO.");

        if (string.IsNullOrWhiteSpace(numeroRemision))
            throw new ReglaDeNegocioException("El número de remisión es obligatorio.");

        Id = id;
        PedidoProveedor = pedidoProveedor;
        UsuarioCreacionId = usuarioCreacionId;
        FechaEntrega = fechaEntrega;
        NumeroRemision = numeroRemision;
        Estado = EntregaEstado.Registrada;
        Observacion = observacion;
    }

    // D-04/RN-046. Regla adicional (cierre técnico 2026-09-11, B1/RN-054): no se permite anular
    // una entrega de un pedido ya CERRADO — un pedido cerrado no debe reabrirse ni recalcularse.
    // Ampliada (cierre 2026-09-11, RN-054): CANCELADO representa un pedido fuera de operación,
    // así que tampoco admite anular sus entregas.
    public void Anular()
    {
        if (Estado != EntregaEstado.Registrada)
            throw new ReglaDeNegocioException("Solo una entrega REGISTRADA puede anularse.");

        if (PedidoProveedor.Estado is PedidoProveedorEstado.Cerrado or PedidoProveedorEstado.Cancelado)
            throw new ReglaDeNegocioException("No se puede anular una entrega de un pedido CERRADO o CANCELADO.");

        Estado = EntregaEstado.Anulada;
    }

    // 04-base-datos.md §30 "Regla": la cantidad entregada acumulada (sumando todas las
    // entregas ya registradas para el mismo DetallePedidoProveedor, no solo esta) no debe
    // superar la cantidad pedida, salvo que exista una regla de negocio explícita que autorice
    // sobrantes (no existe ninguna documentada, así que se bloquea). El acumulado de otras
    // entregas se calcula fuera del agregado (Application, vía IEntregaRepository) porque
    // cruza instancias de Entrega que este objeto no puede alcanzar por sí mismo.
    // Regla adicional (cierre 2026-09-11, RN-054): un pedido CANCELADO está fuera de operación —
    // no admite nuevos detalles en ninguna de sus entregas.
    public DetalleEntrega AgregarDetalle(
        int id, DetallePedidoProveedor detallePedidoOrigen, int cantidadEntregada, int cantidadYaEntregadaEnOtrasEntregas)
    {
        if (PedidoProveedor.Estado == PedidoProveedorEstado.Cancelado)
            throw new ReglaDeNegocioException("No se pueden agregar detalles a una entrega de un pedido CANCELADO.");

        if (detallePedidoOrigen is null || !PedidoProveedor.Detalles.Contains(detallePedidoOrigen))
            throw new ReglaDeNegocioException("El detalle de pedido no pertenece al pedido de esta entrega.");

        if (cantidadYaEntregadaEnOtrasEntregas + cantidadEntregada > detallePedidoOrigen.CantidadPedida)
            throw new ReglaDeNegocioException("La cantidad entregada acumulada no puede superar la cantidad pedida.");

        var detalle = new DetalleEntrega(id, detallePedidoOrigen, cantidadEntregada);
        _detalles.Add(detalle);
        return detalle;
    }

    // RN-035: distribuye una cantidad entregada hacia una sede (con snapshot histórico).
    // Regla adicional (cierre 2026-09-11, RN-054): igual que AgregarDetalle, bloqueada si el
    // pedido está CANCELADO.
    public DistribucionEntrega AgregarDistribucion(int id, DetalleEntrega detalle, Sede sede, int cantidad)
    {
        if (PedidoProveedor.Estado == PedidoProveedorEstado.Cancelado)
            throw new ReglaDeNegocioException("No se pueden agregar distribuciones a una entrega de un pedido CANCELADO.");

        if (detalle is null || !_detalles.Contains(detalle))
            throw new ReglaDeNegocioException("El detalle no pertenece a esta entrega.");

        return detalle.AgregarDistribucion(id, sede, cantidad);
    }
}
