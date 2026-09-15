namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-014, 04-base-datos.md §10.1.
public sealed record CategoriaResponse(int Id, string Nombre, string? Descripcion, bool Activo);
