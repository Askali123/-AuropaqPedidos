namespace AuropaqPedidos.Application.Catalogo.Dtos;

// Información mínima para que el Frontend pueda seleccionar un producto al agregar un detalle de
// Requisición (docs/05-api.md §54.6): nombre e identificación para elegirlo, y la unidad de
// medida porque sin ella la cantidad solicitada no tiene contexto. No incluye Categoria ni
// Descripcion: no son necesarias para seleccionar, solo para catalogar (mínimo necesario,
// instrucción explícita de no ampliar el contrato más de lo pedido).
public sealed record ProductoResponse(
    int Id,
    string Nombre,
    string? CodigoInterno,
    string UnidadMedidaCodigo,
    string UnidadMedidaNombre,
    bool Activo);
