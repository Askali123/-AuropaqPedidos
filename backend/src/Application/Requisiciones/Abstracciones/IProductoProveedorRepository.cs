using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IProductoProveedorRepository
{
    ProductoProveedor? ObtenerPorId(int id);

    // 04-base-datos.md §15 "Restricción": no debe duplicarse la misma relación producto/proveedor.
    bool Existe(int productoId, int proveedorId);

    // 05-api.md §30: GET /productos/{productoId}/proveedores.
    IReadOnlyList<ProductoProveedor> ObtenerPorProducto(int productoId);

    // 05-api.md §30: GET /proveedores/{proveedorId}/productos.
    IReadOnlyList<ProductoProveedor> ObtenerPorProveedor(int proveedorId);

    void Guardar(ProductoProveedor productoProveedor);
}
