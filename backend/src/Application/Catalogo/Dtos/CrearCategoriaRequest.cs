namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-014, 05-api.md §14.
public sealed record CrearCategoriaRequest(string Nombre, string? Descripcion = null);
