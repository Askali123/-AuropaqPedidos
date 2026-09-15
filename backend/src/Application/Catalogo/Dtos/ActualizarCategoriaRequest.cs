namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-014, 05-api.md §14. Activo viaja en el mismo PUT (mismo criterio que Empresa/Sede).
public sealed record ActualizarCategoriaRequest(string Nombre, string? Descripcion, bool Activo);
