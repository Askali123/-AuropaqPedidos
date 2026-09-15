using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CrearSedeUseCaseTests
{
    private sealed class Escenario
    {
        public FakeEmpresaRepository Empresas { get; } = new();
        public FakeSedeRepository Sedes { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public Empresa Empresa { get; } = new(1, "AUROTECH");

        public Escenario() => Empresas.Agregar(Empresa);

        public CrearSedeUseCase UseCase() => new(Empresas, Sedes, Ids);
    }

    [Fact]
    public void Crea_una_sede_activa_asociada_a_la_empresa()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Empresa.Id,
            new CrearSedeRequest("Bogotá", "Calle 1", "Bogotá", "Bogotá D.C.", "6011234567", "Recepción"));

        Assert.Equal("Bogotá", respuesta.Nombre);
        Assert.Equal("Calle 1", respuesta.Direccion);
        Assert.True(respuesta.Activo);
        Assert.NotNull(escenario.Sedes.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_empresa_no_existe()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.UseCase().Ejecutar(
            999, new CrearSedeRequest("Bogotá")));
    }

    [Fact]
    public void No_permite_crear_sede_para_una_empresa_inactiva()
    {
        var escenario = new Escenario();
        escenario.Empresa.Desactivar();

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            escenario.Empresa.Id, new CrearSedeRequest("Bogotá")));
    }
}
