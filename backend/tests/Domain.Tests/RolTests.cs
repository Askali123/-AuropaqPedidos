using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class RolTests
{
    [Fact]
    public void Crear_rol_valido_queda_activo()
    {
        var rol = new Rol(1, "Solicitante", "Puede crear requisiciones");

        Assert.Equal("Solicitante", rol.Nombre);
        Assert.Equal("Puede crear requisiciones", rol.Descripcion);
        Assert.True(rol.Activo);
    }

    [Fact]
    public void Permite_crear_rol_sin_descripcion()
    {
        var rol = new Rol(1, "Solicitante");

        Assert.Null(rol.Descripcion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_rol_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Rol(1, nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var rol = new Rol(1, "Solicitante");

        rol.Desactivar();
        Assert.False(rol.Activo);

        rol.Activar();
        Assert.True(rol.Activo);
    }
}
