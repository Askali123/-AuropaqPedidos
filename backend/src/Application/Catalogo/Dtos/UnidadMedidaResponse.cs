namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-015, 04-base-datos.md §11.
public sealed record UnidadMedidaResponse(int Id, string Codigo, string Nombre, bool Activo);
