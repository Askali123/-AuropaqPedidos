namespace AuropaqPedidos.Domain.Enums;

// Cierre documental 2026-09-11 (D-05, RN-047, ADR-050, 01-reglas-negocio.md §13): estados
// operativos, no contables. Auropaq Pedidos no es un sistema contable (RN-038); no agregar
// CAUSADA/CONTABILIZADA/PAGADA/CONCILIADA sin una nueva decisión documentada.
public enum FacturaEstado
{
    Registrada,
    Anulada
}
