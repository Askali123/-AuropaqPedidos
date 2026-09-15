using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class ActualizarSedeUseCaseTests
{
    private sealed class Escenario
    {
        public FakeSedeRepository Sedes { get; } = new();
        public Empresa Empresa { get; } = new(1, "AUROTECH");
        public Sede Sede { get; }

        public Escenario()
        {
            Sede = new Sede(1, Empresa, "Bogotá", direccion: "Calle 1");
            Sedes.Agregar(Sede);
        }

        public ActualizarSedeUseCase UseCase() => new(Sedes);
    }

    [Fact]
    public void Actualiza_los_datos_de_la_sede()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Sede.Id,
            new ActualizarSedeRequest("Bogotá Norte", "Calle 100", "Bogotá", "Bogotá D.C.", "6011234567", "Recepción", true));

        Assert.Equal("Bogotá Norte", respuesta.Nombre);
        Assert.Equal("Calle 100", respuesta.Direccion);
    }

    [Fact]
    public void No_reasigna_la_empresa_de_la_sede()
    {
        var escenario = new Escenario();

        escenario.UseCase().Ejecutar(
            escenario.Sede.Id,
            new ActualizarSedeRequest("Bogotá Norte", null, null, null, null, null, true));

        Assert.Equal(escenario.Empresa, escenario.Sede.Empresa);
    }

    [Fact]
    public void Puede_desactivar_la_sede()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            escenario.Sede.Id,
            new ActualizarSedeRequest("Bogotá", null, null, null, null, null, false));

        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_sede_no_existe()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.UseCase().Ejecutar(
            999, new ActualizarSedeRequest("X", null, null, null, null, null, true)));
    }

    [Fact]
    public void No_permite_nombre_vacio()
    {
        var escenario = new Escenario();

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            escenario.Sede.Id, new ActualizarSedeRequest("", null, null, null, null, null, true)));
    }
}
