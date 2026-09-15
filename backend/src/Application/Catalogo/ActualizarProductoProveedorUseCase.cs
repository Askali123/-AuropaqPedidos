using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-019, 05-api.md §30: PUT /productos/{productoId}/proveedores/{relacionId}.
public sealed class ActualizarProductoProveedorUseCase
{
    private readonly IProductoProveedorRepository _productosProveedores;

    public ActualizarProductoProveedorUseCase(IProductoProveedorRepository productosProveedores)
    {
        _productosProveedores = productosProveedores;
    }

    public ProductoProveedorResponse Ejecutar(int productoId, int relacionId, ActualizarProductoProveedorRequest request)
    {
        var relacion = _productosProveedores.ObtenerPorId(relacionId)
            ?? throw new RecursoNoEncontradoException($"La relación producto-proveedor {relacionId} no existe.");

        // TASK-051: no confiar en el productoId de la ruta sin verificar que corresponda
        // realmente a esta relación (mismo criterio que RequisicionFinder.ObtenerDetalleOLanzar).
        if (relacion.Producto.Id != productoId)
            throw new RecursoNoEncontradoException($"La relación producto-proveedor {relacionId} no existe.");

        relacion.ActualizarDatos(
            request.CodigoProveedor, request.DescripcionProveedor, request.CategoriaProveedor, request.UnidadProveedor);

        if (request.Activo)
            relacion.Activar();
        else
            relacion.Desactivar();

        _productosProveedores.Guardar(relacion);

        return ProductoProveedorMapper.AResponse(relacion);
    }
}
