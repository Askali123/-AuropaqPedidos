namespace AuropaqPedidos.Api.Common;

// docs/05-api.md §6: toda respuesta de error se envuelve en { "error": { code, message, details } }.
public sealed record ErrorResponse(ErrorDetail Error);

public sealed record ErrorDetail(string Code, string Message, IReadOnlyList<string> Details);
