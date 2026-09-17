using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-046 (incremento MVP + API), decisión provisional de alcance MVP (ver progreso.md, entrada
// "TASK-046 — incremento MVP + API"): para esta demo se fija PedidoProveedor 1 ─── N Facturas
// (Factura.PedidoProveedorId obligatoria). Esta decisión es de alcance MVP y puede evolucionar
// (Decisión 1 de "DECISIONES DE NEGOCIO PENDIENTES — FACTURACIÓN" sigue PENDIENTE DE APROBACIÓN
// para el caso general de facturación consolidada N:N).
//
// Es el agregado raíz de DetalleFactura (mismo criterio que Requisicion/Consolidacion/
// PedidoProveedor/Entrega). Subtotal se calcula como SUM(Detalles.Subtotal) — no se recibe como
// dato de entrada. Impuestos se registra tal como lo indica la factura física del proveedor (sin
// cálculo de tasa — decisión provisional MVP, no se asume IVA). Total = Subtotal + Impuestos.
// Cierre documental 2026-09-11 (D-05/RN-047): Estado es un enum operativo (REGISTRADA/ANULADA),
// sin ciclo contable (RN-038) — reemplaza la propuesta de ciclo de vida contable del 2026-09-10
// (progreso.md, "propuesta de negocio FASE 9"), que quedó explícitamente descartada.
// UsuarioCreacionId agregado 2026-09-17 (P2-2, docs/2026-09-17-tareas.md) — mismo motivo que
// PedidoProveedor.UsuarioCreacionId.
public sealed class Factura
{
    public int Id { get; }
    public Proveedor Proveedor { get; }
    public PedidoProveedor PedidoProveedor { get; }
    public string NumeroFactura { get; }
    public int UsuarioCreacionId { get; }
    public DateTime FechaFactura { get; }
    public decimal Impuestos { get; }
    public FacturaEstado Estado { get; private set; }
    public string? Observacion { get; }

    private readonly List<DetalleFactura> _detalles = new();
    public IReadOnlyList<DetalleFactura> Detalles => _detalles;

    public decimal Subtotal => _detalles.Sum(d => d.Subtotal);
    public decimal Total => Subtotal + Impuestos;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Factura()
    {
    }
#pragma warning restore CS8618

    public Factura(
        int id,
        Proveedor proveedor,
        PedidoProveedor pedidoProveedor,
        string numeroFactura,
        int usuarioCreacionId,
        DateTime fechaFactura,
        decimal impuestos,
        string? observacion = null)
    {
        if (proveedor is null)
            throw new ReglaDeNegocioException("Una factura debe tener un proveedor.");

        if (pedidoProveedor is null)
            throw new ReglaDeNegocioException("Una factura debe estar asociada a un pedido a proveedor.");

        // D-06/RN-039: el proveedor de la factura debe coincidir con el proveedor del pedido
        // asociado (PedidoProveedor 1 ─── N Factura; no hay facturación consolidada entre
        // proveedores ni entre varios pedidos en el alcance actual).
        if (pedidoProveedor.Proveedor.Id != proveedor.Id)
            throw new ReglaDeNegocioException("El proveedor de la factura no corresponde al proveedor del pedido.");

        if (string.IsNullOrWhiteSpace(numeroFactura))
            throw new ReglaDeNegocioException("El número de factura es obligatorio.");

        Id = id;
        Proveedor = proveedor;
        PedidoProveedor = pedidoProveedor;
        NumeroFactura = numeroFactura;
        UsuarioCreacionId = usuarioCreacionId;
        FechaFactura = fechaFactura;
        Impuestos = impuestos;
        Estado = FacturaEstado.Registrada;
        Observacion = observacion;
    }

    // D-05/RN-047.
    public void Anular()
    {
        if (Estado != FacturaEstado.Registrada)
            throw new ReglaDeNegocioException("Solo una factura REGISTRADA puede anularse.");

        Estado = FacturaEstado.Anulada;
    }

    // Decisión provisional de alcance MVP: CantidadFacturada > 0, y la cantidad facturada
    // acumulada para un DetallePedidoProveedor (sumando todas las facturas ya registradas para
    // el mismo pedido, no solo esta) no debe superar CantidadPedida — mismo patrón que
    // Entrega.AgregarDetalle/04-base-datos.md §30, adaptado a facturación. El acumulado de otras
    // facturas se calcula fuera del agregado (Application, vía IFacturaRepository) porque cruza
    // instancias de Factura que este objeto no puede alcanzar por sí mismo.
    public DetalleFactura AgregarDetalle(
        int id, DetallePedidoProveedor detallePedidoOrigen, int cantidadFacturada, decimal precioUnitario, int cantidadYaFacturadaEnOtrasFacturas)
    {
        if (detallePedidoOrigen is null || !PedidoProveedor.Detalles.Contains(detallePedidoOrigen))
            throw new ReglaDeNegocioException("El detalle de pedido no pertenece al pedido de esta factura.");

        if (cantidadYaFacturadaEnOtrasFacturas + cantidadFacturada > detallePedidoOrigen.CantidadPedida)
            throw new ReglaDeNegocioException("La cantidad facturada acumulada no puede superar la cantidad pedida.");

        var detalle = new DetalleFactura(id, detallePedidoOrigen, cantidadFacturada, precioUnitario);
        _detalles.Add(detalle);
        return detalle;
    }
}
