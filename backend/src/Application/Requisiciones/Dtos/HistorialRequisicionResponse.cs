namespace AuropaqPedidos.Application.Requisiciones.Dtos;

public sealed record HistorialRequisicionResponse(
    string EstadoAnterior,
    string EstadoNuevo,
    int UsuarioId,
    DateTime Fecha,
    string? Comentario);
