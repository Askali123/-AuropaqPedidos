namespace AuropaqPedidos.Api.Controllers.Dtos;

// El PedidoProveedorId ya viene en la ruta (POST /pedidos-proveedor/{id}/entregas, docs/05-api.md
// §33); este record solo cubre los campos del cuerpo HTTP para no repetirlo en el body. El
// controller combina route id + este DTO para construir el
// AuropaqPedidos.Application.Entregas.Dtos.CrearEntregaRequest real (mismo criterio que
// AgregarDistribucionPedidoRequest/AgregarDistribucionRequest).
public sealed record CrearEntregaHttpRequest(string NumeroRemision, string? Observacion = null);
