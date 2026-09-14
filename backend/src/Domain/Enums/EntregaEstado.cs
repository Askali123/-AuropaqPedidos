namespace AuropaqPedidos.Domain.Enums;

// Cierre documental 2026-09-11 (D-04, RN-046, ADR-049, 01-reglas-negocio.md §12): estados
// operativos del registro de Entrega, separados de las condiciones de recepción de mercancía
// (Aceptado/Rechazado/Dañado), que quedan diferidas como concepto aparte (Pendiente 4).
public enum EntregaEstado
{
    Registrada,
    Anulada
}
