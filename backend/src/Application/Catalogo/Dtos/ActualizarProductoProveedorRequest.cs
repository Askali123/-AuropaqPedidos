namespace AuropaqPedidos.Application.Catalogo.Dtos;

public sealed record ActualizarProductoProveedorRequest(
    string CodigoProveedor,
    string? DescripcionProveedor,
    string? CategoriaProveedor,
    string? UnidadProveedor,
    bool Activo);
