namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-010. Sin Id: la relación no tiene identidad propia más allá del par (UsuarioId, RolId).
public sealed record UsuarioRolResponse(int UsuarioId, int RolId);
