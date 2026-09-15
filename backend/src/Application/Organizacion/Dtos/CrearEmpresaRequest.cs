namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-006, 05-api.md §11.3.
public sealed record CrearEmpresaRequest(string Nombre, string? Nit = null);
