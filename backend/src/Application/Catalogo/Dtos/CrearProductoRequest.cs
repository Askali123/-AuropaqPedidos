namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-016, 05-api.md §13.3.
public sealed record CrearProductoRequest(
    string Nombre,
    int CategoriaId,
    int UnidadMedidaId,
    string? CodigoInterno = null,
    string? Descripcion = null);
