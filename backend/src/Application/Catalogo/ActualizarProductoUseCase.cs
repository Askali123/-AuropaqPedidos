using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-016, 05-api.md §13.4. Activo se aplica con Activar()/Desactivar(). Categoría/UnidadMedida
// inexistentes -> 404 (mismo criterio que CrearProductoUseCase).
public sealed class ActualizarProductoUseCase
{
    private readonly IProductoRepository _productos;
    private readonly ICategoriaRepository _categorias;
    private readonly IUnidadMedidaRepository _unidadesMedida;

    public ActualizarProductoUseCase(
        IProductoRepository productos,
        ICategoriaRepository categorias,
        IUnidadMedidaRepository unidadesMedida)
    {
        _productos = productos;
        _categorias = categorias;
        _unidadesMedida = unidadesMedida;
    }

    public ProductoResponse Ejecutar(int id, ActualizarProductoRequest request)
    {
        var producto = _productos.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El producto {id} no existe.");

        var categoria = _categorias.ObtenerPorId(request.CategoriaId)
            ?? throw new RecursoNoEncontradoException($"La categoría {request.CategoriaId} no existe.");

        var unidadMedida = _unidadesMedida.ObtenerPorId(request.UnidadMedidaId)
            ?? throw new RecursoNoEncontradoException($"La unidad de medida {request.UnidadMedidaId} no existe.");

        producto.ActualizarDatos(request.Nombre, categoria, unidadMedida, request.CodigoInterno, request.Descripcion);

        if (request.Activo)
            producto.Activar();
        else
            producto.Desactivar();

        _productos.Guardar(producto);

        return new ProductoResponse(
            producto.Id, producto.Nombre, producto.CodigoInterno, producto.Descripcion,
            producto.Categoria.Id, producto.Categoria.Nombre,
            producto.UnidadMedida.Id, producto.UnidadMedida.Codigo, producto.UnidadMedida.Nombre,
            producto.Activo);
    }
}
