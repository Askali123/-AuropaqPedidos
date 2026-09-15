namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-018, 04-base-datos.md §14.
public sealed record ProveedorResponse(
    int Id,
    string Nombre,
    string? Nit,
    string? Contacto,
    string? Telefono,
    string? Correo,
    bool Activo);
