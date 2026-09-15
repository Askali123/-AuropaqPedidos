namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-018, 05-api.md §29.
public sealed record CrearProveedorRequest(
    string Nombre,
    string? Nit = null,
    string? Contacto = null,
    string? Telefono = null,
    string? Correo = null);
