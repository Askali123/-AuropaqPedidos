namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-018, 05-api.md §29. Activo viaja en el mismo PUT (mismo criterio que Empresa/Sede).
public sealed record ActualizarProveedorRequest(
    string Nombre,
    string? Nit,
    string? Contacto,
    string? Telefono,
    string? Correo,
    bool Activo);
