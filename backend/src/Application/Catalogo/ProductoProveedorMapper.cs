using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-019. Mismo patrón que SolicitudProductoCatalogoMapper/RequisicionMapper: evita repetir el
// mapeo en cada caso de uso de ProductoProveedor.
internal static class ProductoProveedorMapper
{
    public static ProductoProveedorResponse AResponse(ProductoProveedor relacion) =>
        new(
            relacion.Id,
            relacion.Producto.Id,
            relacion.Producto.Nombre,
            relacion.Proveedor.Id,
            relacion.Proveedor.Nombre,
            relacion.CodigoProveedor,
            relacion.DescripcionProveedor,
            relacion.CategoriaProveedor,
            relacion.UnidadProveedor,
            relacion.Activo);
}
