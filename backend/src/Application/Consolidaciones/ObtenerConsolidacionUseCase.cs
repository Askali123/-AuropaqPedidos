using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;
using AuropaqPedidos.Application.Excepciones;

namespace AuropaqPedidos.Application.Consolidaciones;

// I1-1 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): consultar el detalle de una
// consolidación (productos agrupados, trazabilidad hacia sus requisiciones de origen).
public sealed class ObtenerConsolidacionUseCase
{
    private readonly IConsolidacionRepository _consolidaciones;

    public ObtenerConsolidacionUseCase(IConsolidacionRepository consolidaciones)
    {
        _consolidaciones = consolidaciones;
    }

    public ConsolidacionResponse Ejecutar(int id)
    {
        var consolidacion = _consolidaciones.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La consolidación {id} no existe.");

        return ConsolidacionMapper.AResponse(consolidacion);
    }
}
