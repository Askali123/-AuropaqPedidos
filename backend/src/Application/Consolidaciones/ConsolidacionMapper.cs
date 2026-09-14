using AuropaqPedidos.Application.Consolidaciones.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Consolidaciones;

// Traduce las entidades de Domain a los DTOs de salida. Ningún caso de uso devuelve
// directamente una entidad de Domain (mismo criterio que RequisicionMapper).
internal static class ConsolidacionMapper
{
    public static ConsolidacionResponse AResponse(Consolidacion consolidacion) => new(
        Id: consolidacion.Id,
        PeriodoId: consolidacion.Periodo.Id,
        UsuarioCreacionId: consolidacion.UsuarioCreacionId,
        Estado: consolidacion.Estado,
        FechaCreacion: consolidacion.FechaCreacion,
        Observacion: consolidacion.Observacion,
        Detalles: consolidacion.Detalles.Select(ADetalleResponse).ToList());

    private static DetalleConsolidacionResponse ADetalleResponse(DetalleConsolidacion detalle) => new(
        Id: detalle.Id,
        ProductoId: detalle.Producto.Id,
        CantidadNecesaria: detalle.CantidadNecesaria,
        Asignaciones: detalle.Asignaciones.Select(AAsignacionResponse).ToList());

    private static AsignacionConsolidacionResponse AAsignacionResponse(AsignacionConsolidacion asignacion) => new(
        Id: asignacion.Id,
        DetalleRequisicionId: asignacion.DetalleRequisicionOrigen.Id,
        Cantidad: asignacion.Cantidad);
}
