namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-017, 05-api.md §28.1. EmpresaId/UsuarioId no viajan en el cuerpo: se obtienen de
// X-Empresa-Id/X-Usuario-Id (IdentidadTemporal), mismo criterio que CrearRequisicionRequest
// mientras no exista autenticación real sobre este recurso.
public sealed record SolicitarProductoRequest(string NombreSolicitado, string? Descripcion = null, string? Observacion = null);
