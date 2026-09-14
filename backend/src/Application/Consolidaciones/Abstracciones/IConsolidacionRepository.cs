using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Consolidaciones.Abstracciones;

public interface IConsolidacionRepository
{
    // TASK-007: CrearPedidoProveedorUseCase necesita cargar la consolidación de origen
    // (con sus Detalles) para poder referenciarla desde PedidoProveedor.
    Consolidacion? ObtenerPorId(int id);

    // A2 (cierre de negocio 2026-09-11): un DetalleRequisicion ya asignado a CUALQUIER
    // Consolidacion (de cualquier periodo) no debe volver a generar necesidad en una nueva
    // consolidación — una Requisicion aprobada participa una sola vez. Se devuelven los Ids de
    // DetalleRequisicion (no de Requisicion) porque es el dato directamente disponible vía
    // AsignacionConsolidacion.DetalleRequisicionOrigen; CrearConsolidacionUseCase decide a nivel
    // de Requisicion completa si ALGUNO de sus detalles ya aparece en este conjunto.
    IReadOnlyList<int> ObtenerIdsDetallesRequisicionYaConsolidados();

    void Guardar(Consolidacion consolidacion);
}
