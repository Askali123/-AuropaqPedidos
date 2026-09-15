namespace AuropaqPedidos.Application.Catalogo.Dtos;

// Ampliado en TASK-016 (CRUD real de Producto, 05-api.md §13) con Descripcion/Categoria — hasta
// entonces solo se usaba para el selector del Frontend de Requisiciones (docs/05-api.md §54.6),
// que ya funciona con estos mismos campos de más (extra ignorados por quien no los necesite).
public sealed record ProductoResponse(
    int Id,
    string Nombre,
    string? CodigoInterno,
    string? Descripcion,
    int CategoriaId,
    string CategoriaNombre,
    int UnidadMedidaId,
    string UnidadMedidaCodigo,
    string UnidadMedidaNombre,
    bool Activo);
