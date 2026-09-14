namespace AuropaqPedidos.Application.Organizacion.Dtos;

// Información mínima para que el Frontend pueda listar/seleccionar una empresa (docs/05-api.md
// §54.6). No expone Nit: no aporta nada a un selector y es un dato interno de la organización.
public sealed record EmpresaResponse(int Id, string Nombre, bool Activo);
