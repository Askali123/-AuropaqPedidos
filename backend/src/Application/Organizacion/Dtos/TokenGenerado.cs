namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-015. Resultado de emitir un JWT — la firma/algoritmo/claims concretos son responsabilidad
// de Infrastructure (IGeneradorDeToken); Application solo necesita el token y su expiración.
public sealed record TokenGenerado(string Token, DateTime FechaExpiracion);
