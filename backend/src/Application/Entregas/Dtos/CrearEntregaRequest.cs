namespace AuropaqPedidos.Application.Entregas.Dtos;

// NumeroRemision se recibe explícito del llamador: mismo criterio que PedidoProveedor.NumeroPedido
// (no hay regla documentada de autogeneración — D-09, 01-reglas-negocio.md §15). Estado ya no se
// recibe: una entrega siempre se crea REGISTRADA (D-04/RN-046).
public sealed record CrearEntregaRequest(int PedidoProveedorId, string NumeroRemision, string? Observacion = null);
