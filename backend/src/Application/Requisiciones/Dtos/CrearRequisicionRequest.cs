namespace AuropaqPedidos.Application.Requisiciones.Dtos;

// Nombre y forma tomados de docs/05-api.md §18/§38.
// EmpresaId deliberadamente no forma parte de este request: debe resolverse desde la
// identidad autenticada (docs/05-api.md §18, docs/06-seguridad.md §5/§6), nunca del cliente.
public sealed record CrearRequisicionRequest(int PeriodoId);
