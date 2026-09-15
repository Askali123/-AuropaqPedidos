namespace AuropaqPedidos.Application.Catalogo.Dtos;

// TASK-017, 04-base-datos.md §13. Estado como string del enum real (mismo criterio que
// RequisicionResponse.Estado: "Pendiente"/"Homologado"/"Creado"/"Rechazado", no mayúsculas).
public sealed record SolicitudProductoCatalogoResponse(
    int Id,
    int EmpresaId,
    int UsuarioId,
    string NombreSolicitado,
    string? Descripcion,
    string? Observacion,
    string Estado,
    int? ProductoResultanteId,
    DateTime FechaSolicitud,
    DateTime? FechaResolucion,
    int? UsuarioResolucionId,
    string? MotivoResolucion);
