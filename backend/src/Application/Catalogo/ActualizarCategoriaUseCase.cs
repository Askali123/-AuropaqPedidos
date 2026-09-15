using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-014, 05-api.md §14.4. Activo se aplica con Activar()/Desactivar() (mismo criterio que
// ActualizarEmpresaUseCase).
public sealed class ActualizarCategoriaUseCase
{
    private readonly ICategoriaRepository _categorias;

    public ActualizarCategoriaUseCase(ICategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    public CategoriaResponse Ejecutar(int id, ActualizarCategoriaRequest request)
    {
        var categoria = _categorias.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La categoría {id} no existe.");

        categoria.ActualizarDatos(request.Nombre, request.Descripcion);

        if (request.Activo)
            categoria.Activar();
        else
            categoria.Desactivar();

        _categorias.Guardar(categoria);

        return new CategoriaResponse(categoria.Id, categoria.Nombre, categoria.Descripcion, categoria.Activo);
    }
}
