namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-007, 05-api.md §12.3. Sin ejemplo de cuerpo documentado para este endpoint; se usa el mismo
// conjunto de campos que la creación (§12.2) más Activo, igual que Empresa (§11.4).
public sealed record ActualizarSedeRequest(
    string Nombre,
    string? Direccion,
    string? Ciudad,
    string? Departamento,
    string? Telefono,
    string? Contacto,
    bool Activo);
