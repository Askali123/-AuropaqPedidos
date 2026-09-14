namespace AuropaqPedidos.Api.Controllers.Dtos;

// AgregarDistribucionPedidoUseCase recibe sedeId/cantidad como parámetros primitivos (no existe
// un DTO de Application para esto todavía, mismo criterio que AgregarDistribucionRequest de
// Requisiciones); este record solo sirve para el binding del cuerpo HTTP.
public sealed record AgregarDistribucionPedidoRequest(int SedeId, int Cantidad);
