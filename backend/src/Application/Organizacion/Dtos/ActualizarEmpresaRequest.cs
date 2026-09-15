namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-006, 05-api.md §11.4. Activo viaja en el mismo PUT (no hay endpoint separado de
// activar/desactivar documentado para Empresa).
public sealed record ActualizarEmpresaRequest(string Nombre, string? Nit, bool Activo);
