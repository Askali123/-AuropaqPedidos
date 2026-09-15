using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class ActualizarEmpresaUseCaseTests
{
    private sealed class Escenario
    {
        public FakeEmpresaRepository Empresas { get; } = new();
        public Empresa Empresa { get; } = new(1, "AUROTECH", "900000000-1");

        public Escenario() => Empresas.Agregar(Empresa);

        public ActualizarEmpresaUseCase UseCase() => new(Empresas);
    }

    [Fact]
    public void Actualiza_nombre_y_nit()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Empresa.Id, new ActualizarEmpresaRequest("AUROTECH S.A.S.", "900111111-2", true));

        Assert.Equal("AUROTECH S.A.S.", respuesta.Nombre);
        Assert.Equal("900111111-2", escenario.Empresa.Nit);
    }

    [Fact]
    public void Puede_desactivar_la_empresa()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Empresa.Id, new ActualizarEmpresaRequest("AUROTECH", "900000000-1", false));

        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Puede_reactivar_una_empresa_inactiva()
    {
        var escenario = new Escenario();
        escenario.Empresa.Desactivar();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Empresa.Id, new ActualizarEmpresaRequest("AUROTECH", "900000000-1", true));

        Assert.True(respuesta.Activo);
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_empresa_no_existe()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.UseCase().Ejecutar(
            999, new ActualizarEmpresaRequest("X", null, true)));
    }

    [Fact]
    public void No_permite_nombre_vacio()
    {
        var escenario = new Escenario();

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            escenario.Empresa.Id, new ActualizarEmpresaRequest("", null, true)));
    }
}
