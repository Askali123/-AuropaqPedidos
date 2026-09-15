using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-016, 05-api.md §13.3: "El backend debe validar: código; categoría; unidad; duplicados;
// datos obligatorios." Categoría/UnidadMedida inexistentes -> 404 (mismo criterio que
// CrearSedeUseCase con Empresa). Sin validación de duplicados de CodigoInterno: su unicidad queda
// pendiente de confirmación del negocio (04-base-datos.md §12, ya documentado en
// ProductoConfiguration) — no se inventa aquí.
public sealed class CrearProductoUseCase
{
    private readonly IProductoRepository _productos;
    private readonly ICategoriaRepository _categorias;
    private readonly IUnidadMedidaRepository _unidadesMedida;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearProductoUseCase(
        IProductoRepository productos,
        ICategoriaRepository categorias,
        IUnidadMedidaRepository unidadesMedida,
        IGeneradorDeIdentificadores ids)
    {
        _productos = productos;
        _categorias = categorias;
        _unidadesMedida = unidadesMedida;
        _ids = ids;
    }

    public ProductoResponse Ejecutar(CrearProductoRequest request)
    {
        var categoria = _categorias.ObtenerPorId(request.CategoriaId)
            ?? throw new RecursoNoEncontradoException($"La categoría {request.CategoriaId} no existe.");

        var unidadMedida = _unidadesMedida.ObtenerPorId(request.UnidadMedidaId)
            ?? throw new RecursoNoEncontradoException($"La unidad de medida {request.UnidadMedidaId} no existe.");

        var producto = new Producto(
            _ids.Siguiente(), request.Nombre, categoria, unidadMedida, request.CodigoInterno, request.Descripcion);

        _productos.Guardar(producto);

        return new ProductoResponse(
            producto.Id, producto.Nombre, producto.CodigoInterno, producto.Descripcion,
            producto.Categoria.Id, producto.Categoria.Nombre,
            producto.UnidadMedida.Id, producto.UnidadMedida.Codigo, producto.UnidadMedida.Nombre,
            producto.Activo);
    }
}
