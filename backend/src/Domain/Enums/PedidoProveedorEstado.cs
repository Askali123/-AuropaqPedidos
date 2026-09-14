namespace AuropaqPedidos.Domain.Enums;

// Cierre documental 2026-09-11 (D-03, RN-043, ADR-046, 01-reglas-negocio.md §11): ciclo de
// estados cerrado como decisión de negocio. No agregar CONFIRMADO/EN_PROCESO/FACTURADO/PAGADO
// sin una nueva decisión documentada.
public enum PedidoProveedorEstado
{
    Borrador,
    Enviado,
    ParcialmenteEntregado,
    Entregado,
    Cerrado,
    Cancelado
}
