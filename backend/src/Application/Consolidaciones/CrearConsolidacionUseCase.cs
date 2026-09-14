using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Consolidaciones;

// TASK-036/TASK-037/TASK-038, RN-027/028/029: toma las requisiciones APROBADAS del periodo
// indicado (de cualquier empresa) que TODAVÍA NO hayan participado en ninguna consolidación
// anterior, y agrupa sus detalles por producto en una nueva Consolidacion, conservando la
// trazabilidad hacia cada DetalleRequisicion de origen sin modificarlo.
//
// A2 (cierre de negocio 2026-09-11): un mismo periodo puede tener múltiples Consolidacion, pero
// una Requisicion aprobada participa en, a lo sumo, una — se excluye por completo (todos sus
// detalles) si CUALQUIERA de sus detalles ya fue asignado en una consolidación previa. Esto
// permite que requisiciones aprobadas *después* de una primera consolidación entren en una
// segunda consolidación del mismo periodo, sin duplicar la necesidad ya consolidada.
public sealed class CrearConsolidacionUseCase
{
    private readonly IConsolidacionRepository _consolidaciones;
    private readonly IRequisicionRepository _requisiciones;
    private readonly IPeriodoRepository _periodos;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearConsolidacionUseCase(
        IConsolidacionRepository consolidaciones,
        IRequisicionRepository requisiciones,
        IPeriodoRepository periodos,
        IGeneradorDeIdentificadores ids)
    {
        _consolidaciones = consolidaciones;
        _requisiciones = requisiciones;
        _periodos = periodos;
        _ids = ids;
    }

    // usuarioId debe provenir de la identidad autenticada, igual que en los casos de uso de
    // Requisicion (docs/06-seguridad.md §5/§6), no del cliente.
    public ConsolidacionResponse Ejecutar(int usuarioId, DateTime fechaActual, CrearConsolidacionRequest request)
    {
        var periodo = _periodos.ObtenerPorId(request.PeriodoId)
            ?? throw new RecursoNoEncontradoException("El periodo indicado no existe.");

        var requisicionesAprobadas = _requisiciones.ObtenerAprobadasPorPeriodo(request.PeriodoId);

        // A2: excluir requisiciones que ya participaron (en su totalidad) en una consolidación
        // anterior — cualquier periodo, no solo este (una Requisicion siempre pertenece a un
        // único Periodo, así que en la práctica el filtro ya queda acotado al periodo actual).
        var detallesYaConsolidados = _consolidaciones.ObtenerIdsDetallesRequisicionYaConsolidados();
        var requisicionesPendientes = requisicionesAprobadas
            .Where(r => !r.Detalles.Any(d => detallesYaConsolidados.Contains(d.Id)))
            .ToList();

        var consolidacion = new Consolidacion(
            id: _ids.Siguiente(),
            periodo: periodo,
            usuarioCreacionId: usuarioId,
            estado: request.Estado,
            fechaCreacion: fechaActual,
            observacion: request.Observacion);

        foreach (var requisicion in requisicionesPendientes)
        {
            foreach (var detalle in requisicion.Detalles)
            {
                consolidacion.AgregarAsignacion(
                    detalleConsolidacionId: _ids.Siguiente(),
                    asignacionId: _ids.Siguiente(),
                    requisicionOrigen: requisicion,
                    detalleOrigen: detalle,
                    cantidad: detalle.CantidadSolicitada);
            }
        }

        _consolidaciones.Guardar(consolidacion);

        return ConsolidacionMapper.AResponse(consolidacion);
    }
}
