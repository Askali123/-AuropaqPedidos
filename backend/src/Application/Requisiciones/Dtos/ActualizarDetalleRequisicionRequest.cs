namespace AuropaqPedidos.Application.Requisiciones.Dtos;

// Nombre tomado de docs/05-api.md §38 ("ActualizarDetalleRequisicionRequest").
// Ambos campos son opcionales para permitir actualizar solo cantidad, solo observación, o ambas
// (docs/05-api.md §20 envía las dos juntas, pero TASK-024 permite modificar cualquiera de ellas).
// Un valor null significa "no modificar ese campo", no "borrarlo".
public sealed record ActualizarDetalleRequisicionRequest(int? CantidadSolicitada, string? Observacion);
