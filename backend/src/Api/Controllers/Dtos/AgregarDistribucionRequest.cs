namespace AuropaqPedidos.Api.Controllers.Dtos;

// AgregarDistribucionRequisicionUseCase recibe sedeId/cantidad como parámetros primitivos
// (no existe un DTO de Application para esto todavía); este record solo sirve para el
// binding del cuerpo HTTP, según docs/05-api.md §22.
public sealed record AgregarDistribucionRequest(int SedeId, int Cantidad);
