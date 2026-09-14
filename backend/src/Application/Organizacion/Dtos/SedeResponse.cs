namespace AuropaqPedidos.Application.Organizacion.Dtos;

// Información mínima para que el Frontend pueda seleccionar una sede al distribuir un detalle de
// Requisición (docs/05-api.md §54.6). No incluye EmpresaId: ya está implícito en la ruta
// GET /api/v1/empresas/{empresaId}/sedes.
public sealed record SedeResponse(int Id, string Nombre, bool Activo);
