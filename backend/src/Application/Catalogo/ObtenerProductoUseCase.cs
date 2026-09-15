using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-016, 05-api.md §13.2.
public sealed class ObtenerProductoUseCase
{
    private readonly IProductoRepository _productos;

    public ObtenerProductoUseCase(IProductoRepository productos)
    {
        _productos = productos;
    }

    public ProductoResponse Ejecutar(int id)
    {
        var producto = _productos.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El producto {id} no existe.");

        return new ProductoResponse(
            producto.Id, producto.Nombre, producto.CodigoInterno, producto.Descripcion,
            producto.Categoria.Id, producto.Categoria.Nombre,
            producto.UnidadMedida.Id, producto.UnidadMedida.Codigo, producto.UnidadMedida.Nombre,
            producto.Activo);
    }
}
