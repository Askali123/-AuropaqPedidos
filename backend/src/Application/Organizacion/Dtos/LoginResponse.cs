namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-015. Reutiliza UsuarioResponse (ya sin PasswordHash) — nunca se expone la contraseña ni
// su hash en ningún DTO de salida (TASK-015 §14).
public sealed record LoginResponse(string Token, DateTime FechaExpiracion, UsuarioResponse Usuario);
