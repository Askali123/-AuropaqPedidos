using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;

namespace AuropaqPedidos.Application.Consolidaciones;

// I1-1 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): permite al Frontend listar
// consolidaciones (opcionalmente filtradas por periodo) en vez de requerir un Id a mano. Sin
// alcance por empresa (RN-064, CLAUDE.md §27): Consolidación no pertenece a una única empresa.
public sealed class ListarConsolidacionesUseCase
{
    private readonly IConsolidacionRepository _consolidaciones;

    public ListarConsolidacionesUseCase(IConsolidacionRepository consolidaciones)
    {
        _consolidaciones = consolidaciones;
    }

    public IReadOnlyList<ConsolidacionResponse> Ejecutar(int? periodoId) =>
        _consolidaciones.Listar(periodoId)
            .Select(ConsolidacionMapper.AResponse)
            .ToList();
}
