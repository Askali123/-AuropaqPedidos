using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones;

// Traduce las entidades de Domain a los DTOs de salida. Ningún caso de uso devuelve
// directamente una entidad de Domain (docs/05-api.md §4/§38).
internal static class RequisicionMapper
{
    public static RequisicionResponse AResponse(Requisicion requisicion) => new(
        Id: requisicion.Id,
        EmpresaId: requisicion.Empresa.Id,
        PeriodoId: requisicion.Periodo.Id,
        UsuarioCreacionId: requisicion.UsuarioCreacionId,
        Estado: requisicion.Estado.ToString(),
        FechaCreacion: requisicion.FechaCreacion,
        FechaEnvio: requisicion.FechaEnvio,
        Detalles: requisicion.Detalles.Select(ADetalleResponse).ToList(),
        Historial: requisicion.Historial.Select(AHistorialResponse).ToList());

    private static DetalleRequisicionResponse ADetalleResponse(DetalleRequisicion detalle) => new(
        Id: detalle.Id,
        ProductoId: detalle.Producto.Id,
        CantidadSolicitada: detalle.CantidadSolicitada,
        Observacion: detalle.Observacion,
        CantidadDistribuida: detalle.CantidadDistribuida,
        DistribucionCompleta: detalle.DistribucionCompleta,
        Distribuciones: detalle.Distribuciones.Select(ADistribucionResponse).ToList());

    private static DistribucionRequisicionResponse ADistribucionResponse(DistribucionRequisicion distribucion) => new(
        Id: distribucion.Id,
        SedeId: distribucion.Sede.Id,
        Cantidad: distribucion.Cantidad);

    private static HistorialRequisicionResponse AHistorialResponse(HistorialRequisicion historial) => new(
        EstadoAnterior: historial.EstadoAnterior.ToString(),
        EstadoNuevo: historial.EstadoNuevo.ToString(),
        UsuarioId: historial.UsuarioId,
        Fecha: historial.Fecha,
        Comentario: historial.Comentario);
}
