using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Periodos.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Periodos;

// TASK-020, 05-api.md §16.2.
public sealed class ObtenerPeriodoUseCase
{
    private readonly IPeriodoRepository _periodos;

    public ObtenerPeriodoUseCase(IPeriodoRepository periodos)
    {
        _periodos = periodos;
    }

    public PeriodoResponse Ejecutar(int id)
    {
        var periodo = _periodos.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El periodo {id} no existe.");

        return new PeriodoResponse(
            periodo.Id,
            periodo.Anio,
            periodo.Mes,
            periodo.FechaInicio,
            periodo.FechaFin,
            periodo.FechaInicioSolicitud,
            periodo.FechaFinSolicitud,
            periodo.Estado);
    }
}
