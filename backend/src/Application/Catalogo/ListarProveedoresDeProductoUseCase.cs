using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-019, 05-api.md §30: GET /productos/{productoId}/proveedores. Mismo criterio que
// ListarSedesPorEmpresaUseCase: si el producto no existe, 404 en vez de una lista vacía.
public sealed class ListarProveedoresDeProductoUseCase
{
    private readonly IProductoRepository _productos;
    private readonly IProductoProveedorRepository _productosProveedores;

    public ListarProveedoresDeProductoUseCase(IProductoRepository productos, IProductoProveedorRepository productosProveedores)
    {
        _productos = productos;
        _productosProveedores = productosProveedores;
    }

    public IReadOnlyList<ProductoProveedorResponse> Ejecutar(int productoId)
    {
        _ = _productos.ObtenerPorId(productoId)
            ?? throw new RecursoNoEncontradoException($"El producto {productoId} no existe.");

        return _productosProveedores.ObtenerPorProducto(productoId)
            .Select(ProductoProveedorMapper.AResponse)
            .ToList();
    }
}
