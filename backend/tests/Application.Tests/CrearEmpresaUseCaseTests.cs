using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;

namespace Application.Tests;

public class CrearEmpresaUseCaseTests
{
    private sealed class Escenario
    {
        public FakeEmpresaRepository Empresas { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();

        public CrearEmpresaUseCase UseCase() => new(Empresas, Ids);
    }

    [Fact]
    public void Crea_una_empresa_activa()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(new CrearEmpresaRequest("AUROTECH", "900000000-1"));

        Assert.Equal("AUROTECH", respuesta.Nombre);
        Assert.True(respuesta.Activo);
        Assert.NotNull(escenario.Empresas.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Permite_crear_una_empresa_sin_nit()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(new CrearEmpresaRequest("AUROTECH"));

        Assert.NotNull(escenario.Empresas.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Asigna_ids_distintos_a_empresas_distintas()
    {
        var escenario = new Escenario();

        var primera = escenario.UseCase().Ejecutar(new CrearEmpresaRequest("AUROTECH"));
        var segunda = escenario.UseCase().Ejecutar(new CrearEmpresaRequest("FAVIPAQ"));

        Assert.NotEqual(primera.Id, segunda.Id);
    }
}
