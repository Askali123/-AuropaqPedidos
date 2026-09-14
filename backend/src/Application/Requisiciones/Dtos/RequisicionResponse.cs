namespace AuropaqPedidos.Application.Requisiciones.Dtos;

// Nombre tomado de docs/05-api.md §38. No expone las entidades de Domain directamente
// (docs/05-api.md §4/§38, docs/03-arquitectura.md §13).
public sealed record RequisicionResponse(
    int Id,
    int EmpresaId,
    int PeriodoId,
    int UsuarioCreacionId,
    string Estado,
    DateTime FechaCreacion,
    DateTime? FechaEnvio,
    IReadOnlyList<DetalleRequisicionResponse> Detalles,
    IReadOnlyList<HistorialRequisicionResponse> Historial);
