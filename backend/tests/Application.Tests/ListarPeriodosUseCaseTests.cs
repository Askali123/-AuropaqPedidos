using Application.Tests.Fakes;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ListarPeriodosUseCaseTests
{
    [Fact]
    public void Devuelve_todos_los_periodos_ordenados_cronologicamente()
    {
        var periodos = new FakePeriodoRepository();
        periodos.Agregar(new Periodo(
            1, 2026, 10,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 31),
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 3),
            "ABIERTO"));
        periodos.Agregar(new Periodo(
            2, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "CERRADO"));

        var respuesta = new ListarPeriodosUseCase(periodos).Ejecutar();

        Assert.Equal(2, respuesta.Count);
        Assert.Equal(9, respuesta[0].Mes);
        Assert.Equal(10, respuesta[1].Mes);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_periodos()
    {
        var respuesta = new ListarPeriodosUseCase(new FakePeriodoRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Incluye_periodos_cerrados_sin_filtrarlos()
    {
        var periodos = new FakePeriodoRepository();
        periodos.Agregar(new Periodo(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "CERRADO"));

        var respuesta = new ListarPeriodosUseCase(periodos).Ejecutar();

        var dto = Assert.Single(respuesta);
        Assert.Equal("CERRADO", dto.Estado);
    }
}
