using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class ProveedorTests
{
    [Fact]
    public void Crear_proveedor_valido_queda_activo()
    {
        var proveedor = new Proveedor(1, "Distribuidora ABC", "900000000-1", "Juan Pérez", "3000000000", "contacto@abc.com");

        Assert.Equal("Distribuidora ABC", proveedor.Nombre);
        Assert.Equal("900000000-1", proveedor.Nit);
        Assert.True(proveedor.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_proveedor_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Proveedor(1, nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var proveedor = new Proveedor(1, "Distribuidora ABC");

        proveedor.Desactivar();
        Assert.False(proveedor.Activo);

        proveedor.Activar();
        Assert.True(proveedor.Activo);
    }

    [Fact]
    public void ActualizarDatos_cambia_los_datos_sin_afectar_el_estado()
    {
        var proveedor = new Proveedor(1, "Distribuidora ABC", "900000000-1");
        proveedor.Desactivar();

        proveedor.ActualizarDatos("Distribuidora ABC S.A.S.", "900111111-2", "María Gómez", "3011111111", "nuevo@abc.com");

        Assert.Equal("Distribuidora ABC S.A.S.", proveedor.Nombre);
        Assert.Equal("900111111-2", proveedor.Nit);
        Assert.Equal("María Gómez", proveedor.Contacto);
        Assert.False(proveedor.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_nombre_vacio(string? nombreInvalido)
    {
        var proveedor = new Proveedor(1, "Distribuidora ABC");

        Assert.Throws<ReglaDeNegocioException>(() => proveedor.ActualizarDatos(nombreInvalido!));
    }
}
