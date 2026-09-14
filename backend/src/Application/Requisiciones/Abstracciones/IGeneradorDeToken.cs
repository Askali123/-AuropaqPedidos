using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// TASK-015. Genera el JWT para un Usuario ya autenticado. La emisión concreta (firma, issuer,
// audience, expiración, claims) es responsabilidad de Infrastructure — Application solo
// coordina el caso de uso (CLAUDE.md §17/TASK-015 §17: "la generación/validación concreta de JWT
// pertenece a la infraestructura de autenticación").
public interface IGeneradorDeToken
{
    TokenGenerado Generar(Usuario usuario, DateTime fecha);
}
