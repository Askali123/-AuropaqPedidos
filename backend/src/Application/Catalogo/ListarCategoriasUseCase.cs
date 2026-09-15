using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-014, 05-api.md §14.1. Devuelve todas sin filtrar por Activo (mismo criterio ya usado en
// ListarEmpresasUseCase/ListarRolesUseCase).
public sealed class ListarCategoriasUseCase
{
    private readonly ICategoriaRepository _categorias;

    public ListarCategoriasUseCase(ICategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    public IReadOnlyList<CategoriaResponse> Ejecutar() =>
        _categorias.ObtenerTodas()
            .Select(c => new CategoriaResponse(c.Id, c.Nombre, c.Descripcion, c.Activo))
            .ToList();
}
