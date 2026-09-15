using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ObtenerPeriodoUseCaseTests
{
    [Fact]
    public void Devuelve_el_periodo_solicitado()
    {
        var periodos = new FakePeriodoRepository();
        var periodo = new Periodo(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO");
        periodos.Agregar(periodo);

        var respuesta = new ObtenerPeriodoUseCase(periodos).Ejecutar(periodo.Id);

        Assert.Equal(2026, respuesta.Anio);
        Assert.Equal(9, respuesta.Mes);
        Assert.Equal("ABIERTO", respuesta.Estado);
    }

    [Fact]
    public void Lanza_no_encontrado_si_el_periodo_no_existe()
    {
        var periodos = new FakePeriodoRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerPeriodoUseCase(periodos).Ejecutar(999));
    }
}
