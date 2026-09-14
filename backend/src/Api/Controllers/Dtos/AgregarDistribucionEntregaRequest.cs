namespace AuropaqPedidos.Api.Controllers.Dtos;

// AgregarDistribucionEntregaUseCase recibe sedeId/cantidad como parámetros primitivos (no existe
// un DTO de Application para esto todavía, mismo criterio que AgregarDistribucionPedidoRequest);
// este record solo sirve para el binding del cuerpo HTTP.
public sealed record AgregarDistribucionEntregaRequest(int SedeId, int Cantidad);
