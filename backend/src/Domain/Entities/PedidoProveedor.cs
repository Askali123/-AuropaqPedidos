using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-030/RN-031, 02-dominio.md §21, 04-base-datos.md §26: representa la compra solicitada a
// UN proveedor, originada en una Consolidacion. No pertenece a una Empresa (un pedido puede
// abastecer varias empresas — 02-dominio.md §21/§33). Es el agregado raíz de
// DetallePedidoProveedor/DistribucionPedido, mismo criterio que Requisicion/Consolidacion.
//
// Decisiones cerradas en el refinamiento/cierre documental 2026-09-11 (01-reglas-negocio.md
// §11/§15, D-01/D-03/D-08): el Proveedor y el NumeroPedido se reciben como parámetros
// explícitos del llamador (D-01) — esta clase no selecciona automáticamente un proveedor ni
// genera un número de pedido. Estado es un enum con ciclo cerrado (D-03/RN-043) y transiciones
// controladas (Enviar/ActualizarEstadoPorEntregas/Cerrar/Cancelar, D-08/RN-044); un pedido
// siempre se crea en BORRADOR.
//
// UsuarioCreacionId agregado 2026-09-17 (P2-2, docs/2026-09-17-tareas.md): RN-050/D-11 pospuso
// este campo hasta que existiera autenticación real (TASK-008 + Fase 10) — ya existe desde
// 2026-09-15, y desde el mismo día (P1, RN-063/ADR-066) el JWT es obligatorio en
// `PedidosProveedorController`, así que siempre hay un usuario autenticado disponible.
public sealed class PedidoProveedor
{
    public int Id { get; }
    public Consolidacion Consolidacion { get; }
    public Proveedor Proveedor { get; }
    public string NumeroPedido { get; }
    public int UsuarioCreacionId { get; }
    public DateTime FechaPedido { get; }
    public DateTime? FechaEntregaEstimada { get; }
    public PedidoProveedorEstado Estado { get; private set; }
    public string? Observacion { get; }

    private readonly List<DetallePedidoProveedor> _detalles = new();
    public IReadOnlyList<DetallePedidoProveedor> Detalles => _detalles;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private PedidoProveedor()
    {
    }
#pragma warning restore CS8618

    public PedidoProveedor(
        int id,
        Consolidacion consolidacion,
        Proveedor proveedor,
        string numeroPedido,
        int usuarioCreacionId,
        DateTime fechaPedido,
        DateTime? fechaEntregaEstimada = null,
        string? observacion = null)
    {
        if (consolidacion is null)
            throw new ReglaDeNegocioException("Un pedido debe originarse en una consolidación.");

        if (proveedor is null)
            throw new ReglaDeNegocioException("Un pedido debe tener un proveedor.");

        if (string.IsNullOrWhiteSpace(numeroPedido))
            throw new ReglaDeNegocioException("El número de pedido es obligatorio.");

        Id = id;
        Consolidacion = consolidacion;
        Proveedor = proveedor;
        NumeroPedido = numeroPedido;
        UsuarioCreacionId = usuarioCreacionId;
        FechaPedido = fechaPedido;
        Estado = PedidoProveedorEstado.Borrador;
        FechaEntregaEstimada = fechaEntregaEstimada;
        Observacion = observacion;
    }

    // D-03/RN-043: BORRADOR -> ENVIADO. RN-065/D-18 (2026-09-17): la distribución de cada
    // detalle debe estar completa antes de enviar — mismo criterio que Requisicion.Enviar()
    // (RN-011).
    public void Enviar()
    {
        if (Estado != PedidoProveedorEstado.Borrador)
            throw new ReglaDeNegocioException("Solo un pedido en BORRADOR puede enviarse.");

        if (_detalles.Any(d => !d.DistribucionCompleta))
            throw new ReglaDeNegocioException(
                "Todos los detalles deben tener su cantidad pedida completamente distribuida entre sedes antes de enviar.");

        Estado = PedidoProveedorEstado.Enviado;
    }

    // D-03/RN-043, ampliado por B1 (cierre técnico 2026-09-11, anulación de Entrega): recalcula
    // ENVIADO/PARCIALMENTE_ENTREGADO/ENTREGADO según las entregas VÁLIDAS (no ANULADAS)
    // restantes. "hayAlgunaCantidadEntregada" distingue el caso en que ya no queda ninguna
    // cantidad entregada válida (p. ej. se anuló la única entrega): ahí el pedido vuelve a
    // ENVIADO, porque RN-043 define PARCIALMENTE_ENTREGADO como "existe al menos una entrega".
    // El cálculo de ambos booleanos cruza Entrega (otro agregado) y vive en Application
    // (AgregarDetalleEntregaUseCase/AnularEntregaUseCase), mismo criterio que
    // CalcularCantidadPendienteUseCase.
    public void ActualizarEstadoPorEntregas(bool hayAlgunaCantidadEntregada, bool quedaCantidadPendiente)
    {
        if (Estado is PedidoProveedorEstado.Borrador or PedidoProveedorEstado.Cerrado or PedidoProveedorEstado.Cancelado)
            throw new ReglaDeNegocioException(
                "Solo un pedido enviado (o con entregas ya registradas) puede actualizar su estado según las entregas.");

        Estado = !hayAlgunaCantidadEntregada
            ? PedidoProveedorEstado.Enviado
            : quedaCantidadPendiente ? PedidoProveedorEstado.ParcialmenteEntregado : PedidoProveedorEstado.Entregado;
    }

    // D-08/RN-044: ENTREGADO -> CERRADO. La factura no es requisito (RN-040).
    public void Cerrar()
    {
        if (Estado != PedidoProveedorEstado.Entregado)
            throw new ReglaDeNegocioException("Solo un pedido en estado ENTREGADO puede cerrarse.");

        Estado = PedidoProveedorEstado.Cerrado;
    }

    // D-03/RN-043: BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO -> CANCELADO.
    public void Cancelar()
    {
        if (Estado is not (PedidoProveedorEstado.Borrador or PedidoProveedorEstado.Enviado or PedidoProveedorEstado.ParcialmenteEntregado))
            throw new ReglaDeNegocioException(
                "Solo un pedido en BORRADOR, ENVIADO o PARCIALMENTE_ENTREGADO puede cancelarse.");

        Estado = PedidoProveedorEstado.Cancelado;
    }

    // RN-031: CantidadNecesaria queda fija como fotografía del detalle consolidado en el
    // momento de agregarse (RN-028: nunca se modifica el origen); CantidadPedida puede diferir
    // (mayor, menor o igual) — ninguna regla documentada restringe esa diferencia ni exige
    // autorización (Pendiente 6 de 01-reglas-negocio.md §14), así que no se valida aquí.
    // detalleConsolidacionOrigen debe pertenecer a la misma Consolidacion de este pedido.
    public DetallePedidoProveedor AgregarDetalle(int id, DetalleConsolidacion detalleConsolidacionOrigen, int cantidadPedida, decimal? precioUnitario = null)
    {
        if (detalleConsolidacionOrigen is null || !Consolidacion.Detalles.Contains(detalleConsolidacionOrigen))
            throw new ReglaDeNegocioException("El detalle de consolidación no pertenece a la consolidación de este pedido.");

        var detalle = new DetallePedidoProveedor(
            id, detalleConsolidacionOrigen.Producto, detalleConsolidacionOrigen.CantidadNecesaria, cantidadPedida, precioUnitario);

        _detalles.Add(detalle);
        return detalle;
    }

    // RN-032: distribuye una cantidad del detalle hacia una sede (de cualquier empresa).
    public DistribucionPedido AgregarDistribucion(int id, DetallePedidoProveedor detalle, Sede sede, int cantidad)
    {
        if (detalle is null || !_detalles.Contains(detalle))
            throw new ReglaDeNegocioException("El detalle no pertenece a este pedido.");

        return detalle.AgregarDistribucion(id, sede, cantidad);
    }
}
