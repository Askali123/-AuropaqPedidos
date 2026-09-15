using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-014, 05-api.md §14.3. Sin validación de unicidad de Nombre: 04-base-datos.md §10.1 no la
// documenta (mismo criterio ya aplicado a Empresa.Nit/Rol.Nombre).
public sealed class CrearCategoriaUseCase
{
    private readonly ICategoriaRepository _categorias;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearCategoriaUseCase(ICategoriaRepository categorias, IGeneradorDeIdentificadores ids)
    {
        _categorias = categorias;
        _ids = ids;
    }

    public CategoriaResponse Ejecutar(CrearCategoriaRequest request)
    {
        var categoria = new Categoria(_ids.Siguiente(), request.Nombre, request.Descripcion);

        _categorias.Guardar(categoria);

        return new CategoriaResponse(categoria.Id, categoria.Nombre, categoria.Descripcion, categoria.Activo);
    }
}
