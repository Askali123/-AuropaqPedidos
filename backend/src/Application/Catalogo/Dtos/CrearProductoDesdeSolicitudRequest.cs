namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-017, 05-api.md §28.4. §28.4 no documenta un cuerpo de request; se completa con el mínimo
// que exige el invariante de Producto (categoría y unidad de medida son obligatorias, RN-021) —
// mismo criterio ya usado para CrearProductoRequest (TASK-016). Nombre/Descripcion del producto
// nuevo se toman de la propia solicitud (NombreSolicitado/Descripcion), no se piden aquí: son la
// razón de ser de la solicitud, no un dato nuevo a decidir en este paso.
public sealed record CrearProductoDesdeSolicitudRequest(int CategoriaId, int UnidadMedidaId, string? CodigoInterno = null);
