using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-019, 05-api.md §30: POST /productos/{productoId}/proveedores. Producto/Proveedor
// inexistentes -> 404 (mismo criterio que CrearProductoUseCase con Categoría/UnidadMedida).
// Relación duplicada -> 422 (04-base-datos.md §15 "Restricción").
public sealed class AsociarProveedorAProductoUseCase
{
    private readonly IProductoProveedorRepository _productosProveedores;
    private readonly IProductoRepository _productos;
    private readonly IProveedorRepository _proveedores;
    private readonly IGeneradorDeIdentificadores _ids;

    public AsociarProveedorAProductoUseCase(
        IProductoProveedorRepository productosProveedores, IProductoRepository productos,
        IProveedorRepository proveedores, IGeneradorDeIdentificadores ids)
    {
        _productosProveedores = productosProveedores;
        _productos = productos;
        _proveedores = proveedores;
        _ids = ids;
    }

    public ProductoProveedorResponse Ejecutar(int productoId, CrearProductoProveedorRequest request)
    {
        var producto = _productos.ObtenerPorId(productoId)
            ?? throw new RecursoNoEncontradoException($"El producto {productoId} no existe.");

        var proveedor = _proveedores.ObtenerPorId(request.ProveedorId)
            ?? throw new RecursoNoEncontradoException($"El proveedor {request.ProveedorId} no existe.");

        if (_productosProveedores.Existe(productoId, proveedor.Id))
            throw new ReglaDeNegocioException("Ya existe una relación entre este producto y este proveedor.");

        var relacion = new ProductoProveedor(
            _ids.Siguiente(), producto, proveedor, request.CodigoProveedor,
            request.DescripcionProveedor, request.CategoriaProveedor, request.UnidadProveedor);

        _productosProveedores.Guardar(relacion);

        return ProductoProveedorMapper.AResponse(relacion);
    }
}
