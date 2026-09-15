namespace AuropaqPedidos.Application.Catalogo.Dtos;

public sealed record ProductoProveedorResponse(
    int Id,
    int ProductoId,
    string ProductoNombre,
    int ProveedorId,
    string ProveedorNombre,
    string CodigoProveedor,
    string? DescripcionProveedor,
    string? CategoriaProveedor,
    string? UnidadProveedor,
    bool Activo);
