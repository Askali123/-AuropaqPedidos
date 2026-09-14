using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class PermisoTests
{
    [Fact]
    public void Crear_permiso_valido()
    {
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición", "Permite crear una requisición mensual");

        Assert.Equal("REQUISICION_CREAR", permiso.Codigo);
        Assert.Equal("Crear requisición", permiso.Nombre);
        Assert.Equal("Permite crear una requisición mensual", permiso.Descripcion);
    }

    [Fact]
    public void Permite_crear_permiso_sin_descripcion()
    {
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");

        Assert.Null(permiso.Descripcion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_permiso_sin_codigo(string? codigoInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Permiso(1, codigoInvalido!, "Crear requisición"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_permiso_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Permiso(1, "REQUISICION_CREAR", nombreInvalido!));
    }
}
