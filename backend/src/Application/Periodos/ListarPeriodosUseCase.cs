using AuropaqPedidos.Application.Periodos.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Periodos;

// Habilita el selector de periodo del Frontend de Requisiciones (docs/05-api.md §54.6). Devuelve
// todos los periodos sin filtrar por Estado ni por ventana de solicitud: ninguna regla de negocio
// define ese filtro para esta consulta (la única validación de ventana ya existente es
// Requisicion.Enviar(), no la de listar periodos) — instrucción explícita de no inventarla aquí.
public sealed class ListarPeriodosUseCase
{
    private readonly IPeriodoRepository _periodos;

    public ListarPeriodosUseCase(IPeriodoRepository periodos)
    {
        _periodos = periodos;
    }

    public IReadOnlyList<PeriodoResponse> Ejecutar() =>
        _periodos.ObtenerTodos()
            .Select(p => new PeriodoResponse(
                p.Id, p.Anio, p.Mes, p.FechaInicio, p.FechaFin, p.FechaInicioSolicitud, p.FechaFinSolicitud, p.Estado))
            .ToList();
}
