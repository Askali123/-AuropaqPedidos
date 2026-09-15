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

    [Fact]
    public void ActualizarDatos_cambia_codigo_y_nombre_sin_afectar_el_estado()
    {
        var unidad = new UnidadMedida(1, "CAJA", "Caja");
        unidad.Desactivar();

        unidad.ActualizarDatos("CJA", "Caja grande");

        Assert.Equal("CJA", unidad.Codigo);
        Assert.Equal("Caja grande", unidad.Nombre);
        Assert.False(unidad.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_codigo_vacio(string? codigoInvalido)
    {
        var unidad = new UnidadMedida(1, "CAJA", "Caja");

        Assert.Throws<ReglaDeNegocioException>(() => unidad.ActualizarDatos(codigoInvalido!, "Caja"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_nombre_vacio(string? nombreInvalido)
    {
        var unidad = new UnidadMedida(1, "CAJA", "Caja");

        Assert.Throws<ReglaDeNegocioException>(() => unidad.ActualizarDatos("CAJA", nombreInvalido!));
    }
}
