namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-007, 05-api.md §12.2. El campo "activo" del ejemplo de la documentación no se recibe aquí:
// una Sede nueva siempre nace Activo=true (mismo criterio que Empresa/Usuario) — no hay una regla
// de negocio que permita crear una sede ya inactiva.
public sealed record CrearSedeRequest(
    string Nombre,
    string? Direccion = null,
    string? Ciudad = null,
    string? Departamento = null,
    string? Telefono = null,
    string? Contacto = null);
