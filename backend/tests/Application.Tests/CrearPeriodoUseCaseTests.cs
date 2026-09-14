using Application.Tests.Fakes;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Application.Periodos.Dtos;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CrearPeriodoUseCaseTests
{
    private static CrearPeriodoRequest Request(int anio = 2026, int mes = 11) => new(
        anio, mes,
        new DateTime(anio, mes, 1), new DateTime(anio, mes, 28),
        new DateTime(anio, mes, 1), new DateTime(anio, mes, 3));

    [Fact]
    public void Crea_un_periodo_con_estado_ABIERTO()
    {
        var periodos = new FakePeriodoRepository();

        var respuesta = new CrearPeriodoUseCase(periodos, new FakeGeneradorDeIdentificadores())
            .Ejecutar(Request());

        Assert.Equal("ABIERTO", respuesta.Estado);
        Assert.Equal(2026, respuesta.Anio);
        Assert.Equal(11, respuesta.Mes);
        Assert.NotNull(periodos.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void No_permite_crear_dos_periodos_para_el_mismo_anio_y_mes()
    {
        var periodos = new FakePeriodoRepository();
        var useCase = new CrearPeriodoUseCase(periodos, new FakeGeneradorDeIdentificadores());
        useCase.Ejecutar(Request());

        Assert.Throws<ReglaDeNegocioException>(() => useCase.Ejecutar(Request()));
    }

    [Fact]
    public void Permite_crear_periodos_de_distinto_anio_o_mes()
    {
        var periodos = new FakePeriodoRepository();
        var useCase = new CrearPeriodoUseCase(periodos, new FakeGeneradorDeIdentificadores());
        useCase.Ejecutar(Request(2026, 11));

        var respuesta = useCase.Ejecutar(Request(2026, 12));

        Assert.Equal(12, respuesta.Mes);
        Assert.Equal(2, periodos.ObtenerTodos().Count);
    }
}
