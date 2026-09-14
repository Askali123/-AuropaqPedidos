namespace AuropaqPedidos.Domain.Enums;

// Subconjunto de 04-base-datos.md §18 correspondiente al flujo ya autorizado del MVP
// (docs/08-tareas.md §20/§21). CONSOLIDADA/EN_PEDIDO/EN_ENTREGA/CERRADA quedan fuera de
// alcance hasta que se autoricen las fases de Consolidación/Pedidos/Entregas/Facturación.
public enum RequisicionEstado
{
    Borrador,
    Enviada,
    EnRevision,
    Devuelta,
    Aprobada
}
