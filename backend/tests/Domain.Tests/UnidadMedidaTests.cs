using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class UnidadMedidaTests
{
    [Fact]
    public void Crear_unidad_valida_queda_activa()
    {
        var unidad = new UnidadMedida(1, "GALON", "Galón");

        Assert.Equal("GALON", unidad.Codigo);
        Assert.Equal("Galón", unidad.Nombre);
        Assert.True(unidad.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_unidad_sin_codigo(string? codigoInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new UnidadMedida(1, codigoInvalido!, "Galón"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_unidad_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new UnidadMedida(1, "GALON", nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var unidad = new UnidadMedida(1, "CAJA", "Caja");

        unidad.Desactivar();
        Assert.False(unidad.Activo);

        unidad.Activar();
        Assert.True(unidad.Activo);
    }
}
