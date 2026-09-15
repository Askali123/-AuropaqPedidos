using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-019, 05-api.md §30: GET /proveedores/{proveedorId}/productos. Mismo criterio que
// ListarSedesPorEmpresaUseCase: si el proveedor no existe, 404 en vez de una lista vacía.
public sealed class ListarProductosDeProveedorUseCase
{
    private readonly IProveedorRepository _proveedores;
    private readonly IProductoProveedorRepository _productosProveedores;

    public ListarProductosDeProveedorUseCase(IProveedorRepository proveedores, IProductoProveedorRepository productosProveedores)
    {
        _proveedores = proveedores;
        _productosProveedores = productosProveedores;
    }

    public IReadOnlyList<ProductoProveedorResponse> Ejecutar(int proveedorId)
    {
        _ = _proveedores.ObtenerPorId(proveedorId)
            ?? throw new RecursoNoEncontradoException($"El proveedor {proveedorId} no existe.");

        return _productosProveedores.ObtenerPorProveedor(proveedorId)
            .Select(ProductoProveedorMapper.AResponse)
            .ToList();
    }
}
