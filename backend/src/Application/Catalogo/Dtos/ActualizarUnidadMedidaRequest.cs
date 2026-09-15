namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-015, 05-api.md §15. Activo viaja en el mismo PUT (mismo criterio que Empresa/Sede).
public sealed record ActualizarUnidadMedidaRequest(string Codigo, string Nombre, bool Activo);
