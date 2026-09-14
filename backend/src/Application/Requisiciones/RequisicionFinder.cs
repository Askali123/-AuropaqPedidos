using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones;

// Búsquedas compartidas por los casos de uso de Requisicion. Centraliza las excepciones
// "no encontrado" (404) para no repetirlas en cada caso de uso.
internal static class RequisicionFinder
{
    public static Requisicion ObtenerOLanzar(IRequisicionRepository repositorio, int requisicionId)
    {
        return repositorio.ObtenerPorId(requisicionId)
            ?? throw new RecursoNoEncontradoException($"La requisición {requisicionId} no existe.");
    }

    public static DetalleRequisicion ObtenerDetalleOLanzar(Requisicion requisicion, int detalleId)
    {
        return requisicion.Detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new RecursoNoEncontradoException(
                $"El detalle {detalleId} no existe en la requisición {requisicion.Id}.");
    }

    public static DistribucionRequisicion ObtenerDistribucionOLanzar(DetalleRequisicion detalle, int distribucionId)
    {
        return detalle.Distribuciones.FirstOrDefault(d => d.Id == distribucionId)
            ?? throw new RecursoNoEncontradoException(
                $"La distribución {distribucionId} no existe en el detalle {detalle.Id}.");
    }
}
