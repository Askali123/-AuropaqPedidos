using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace Application.Tests.Fakes;

internal sealed class FakeGeneradorDeIdentificadores : IGeneradorDeIdentificadores
{
    private int _siguiente = 1;

    public int Siguiente() => _siguiente++;
}
