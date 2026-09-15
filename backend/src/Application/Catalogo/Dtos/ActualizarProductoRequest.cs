namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-016, 05-api.md §13.4. Activo viaja en el mismo PUT (mismo criterio que Empresa/Sede).
public sealed record ActualizarProductoRequest(
    string Nombre,
    int CategoriaId,
    int UnidadMedidaId,
    string? CodigoInterno,
    string? Descripcion,
    bool Activo);
