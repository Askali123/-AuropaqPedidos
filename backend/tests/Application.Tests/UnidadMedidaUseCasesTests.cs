using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class UnidadMedidaUseCasesTests
{
    [Fact]
    public void Crea_una_unidad_de_medida_activa()
    {
        var unidadesMedida = new FakeUnidadMedidaRepository();
        var ids = new FakeGeneradorDeIdentificadores();

        var respuesta = new CrearUnidadMedidaUseCase(unidadesMedida, ids).Ejecutar(new CrearUnidadMedidaRequest("GALON", "Galón"));

        Assert.Equal("GALON", respuesta.Codigo);
        Assert.True(respuesta.Activo);
        Assert.NotNull(unidadesMedida.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Obtiene_una_unidad_de_medida_existente()
    {
        var unidadesMedida = new FakeUnidadMedidaRepository();
        var unidad = new UnidadMedida(1, "GALON", "Galón");
        unidadesMedida.Agregar(unidad);

        var respuesta = new ObtenerUnidadMedidaUseCase(unidadesMedida).Ejecutar(unidad.Id);

        Assert.Equal("GALON", respuesta.Codigo);
    }

    [Fact]
    public void Obtener_unidad_de_medida_inexistente_lanza_no_encontrado()
    {
        var unidadesMedida = new FakeUnidadMedidaRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerUnidadMedidaUseCase(unidadesMedida).Ejecutar(999));
    }

    [Fact]
    public void Actualiza_datos_y_puede_desactivar_una_unidad_de_medida()
    {
        var unidadesMedida = new FakeUnidadMedidaRepository();
        var unidad = new UnidadMedida(1, "GALON", "Galón");
        unidadesMedida.Agregar(unidad);

        var respuesta = new ActualizarUnidadMedidaUseCase(unidadesMedida).Ejecutar(
            unidad.Id, new ActualizarUnidadMedidaRequest("GAL", "Galón US", false));

        Assert.Equal("GAL", respuesta.Codigo);
        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Actualizar_unidad_de_medida_inexistente_lanza_no_encontrado()
    {
        var unidadesMedida = new FakeUnidadMedidaRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ActualizarUnidadMedidaUseCase(unidadesMedida).Ejecutar(
            999, new ActualizarUnidadMedidaRequest("X", "X", true)));
    }
}
