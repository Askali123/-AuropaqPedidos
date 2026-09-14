namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-013. Sin Id: la relación no tiene identidad propia más allá del par (RolId, PermisoId).
public sealed record RolPermisoResponse(int RolId, int PermisoId);
