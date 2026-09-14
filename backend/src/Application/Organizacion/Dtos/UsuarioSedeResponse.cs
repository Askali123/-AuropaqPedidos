namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-009. Sin Id: la relación no tiene identidad propia más allá del par (UsuarioId, SedeId).
public sealed record UsuarioSedeResponse(int UsuarioId, int SedeId);
