using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-015, 05-api.md §15. Devuelve todas sin filtrar por Activo.
public sealed class ListarUnidadesMedidaUseCase
{
    private readonly IUnidadMedidaRepository _unidadesMedida;

    public ListarUnidadesMedidaUseCase(IUnidadMedidaRepository unidadesMedida)
    {
        _unidadesMedida = unidadesMedida;
    }

    public IReadOnlyList<UnidadMedidaResponse> Ejecutar() =>
        _unidadesMedida.ObtenerTodas()
            .Select(u => new UnidadMedidaResponse(u.Id, u.Codigo, u.Nombre, u.Activo))
            .ToList();
}
