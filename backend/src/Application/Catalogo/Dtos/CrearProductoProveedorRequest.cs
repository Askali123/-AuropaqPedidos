namespace AuropaqPedidos.Application.Catalogo.Dtos;

public sealed record CrearProductoProveedorRequest(
    int ProveedorId,
    string CodigoProveedor,
    string? DescripcionProveedor = null,
    string? CategoriaProveedor = null,
    string? UnidadProveedor = null);
