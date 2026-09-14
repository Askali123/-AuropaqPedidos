namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-011, 04-base-datos.md §9.2. Permiso es GLOBAL: sin EmpresaId (el documento no lo
// declara). Sin Activo: a diferencia de RolResponse, el documento no lista ese campo aquí.
public sealed record PermisoResponse(int Id, string Codigo, string Nombre, string? Descripcion);
