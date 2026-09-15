using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-014, 05-api.md §14.2.
public sealed class ObtenerCategoriaUseCase
{
    private readonly ICategoriaRepository _categorias;

    public ObtenerCategoriaUseCase(ICategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    public CategoriaResponse Ejecutar(int id)
    {
        var categoria = _categorias.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La categoría {id} no existe.");

        return new CategoriaResponse(categoria.Id, categoria.Nombre, categoria.Descripcion, categoria.Activo);
    }
}
