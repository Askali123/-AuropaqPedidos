namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-010, 04-base-datos.md §9.1. Rol es GLOBAL: sin EmpresaId (el documento no lo declara).
public sealed record RolResponse(int Id, string Nombre, string? Descripcion, bool Activo);
