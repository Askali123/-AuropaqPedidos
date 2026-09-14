using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

// TASK-015. No genera un JWT real (no hace falta para probar Application) — un valor
// determinista y verificable es suficiente para comprobar que LoginUseCase delega en esta
// abstracción y propaga su resultado.
internal sealed class FakeGeneradorDeToken : IGeneradorDeToken
{
    public TokenGenerado Generar(Usuario usuario, DateTime fecha) =>
        new($"token-de-prueba-usuario-{usuario.Id}", fecha.AddMinutes(60));
}
