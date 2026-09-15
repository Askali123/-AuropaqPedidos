namespace AuropaqPedidos.Application.Organizacion.Dtos;

// Ampliado en TASK-007 (CRUD real de Sede, 05-api.md §12) con los campos de ubicación — hasta
// entonces solo se usaba para el selector del Frontend de Requisiciones (docs/05-api.md §54.6),
// que ya funciona con estos mismos campos de más (extra ignorados por quien no los necesite). No
// incluye EmpresaId: ya está implícito en la ruta GET /api/v1/empresas/{empresaId}/sedes.
public sealed record SedeResponse(
    int Id,
    string Nombre,
    string? Direccion,
    string? Ciudad,
    string? Departamento,
    string? Telefono,
    string? Contacto,
    bool Activo);
