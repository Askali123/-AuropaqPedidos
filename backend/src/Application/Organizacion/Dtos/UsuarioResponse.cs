namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-008, 04-base-datos.md §7. A diferencia de EmpresaResponse/SedeResponse (selectores
// mínimos), este listado no está anidado bajo una Empresa concreta, así que sí expone
// EmpresaId. Sin ningún campo de autenticación (no hay contraseña que exponer).
public sealed record UsuarioResponse(
    int Id,
    int EmpresaId,
    string Nombre,
    string? Apellido,
    string Correo,
    bool Activo,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
