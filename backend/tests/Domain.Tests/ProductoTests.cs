using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class ProductoTests
{
    private static Categoria CrearCategoria() => new(1, "Papelería");
    private static UnidadMedida CrearUnidad() => new(1, "UNIDAD", "Unidad");

    [Fact]
    public void Crear_producto_valido_queda_activo_y_asociado()
    {
        var categoria = CrearCategoria();
        var unidad = CrearUnidad();

        var producto = new Producto(1, "Papel higiénico", categoria, unidad, codigoInterno: "PAP-001");

        Assert.Equal("Papel higiénico", producto.Nombre);
        Assert.Equal(categoria, producto.Categoria);
        Assert.Equal(unidad, producto.UnidadMedida);
        Assert.Equal("PAP-001", producto.CodigoInterno);
        Assert.True(producto.Activo);
    }

    [Fact]
    public void Permite_crear_producto_sin_codigo_interno()
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad());

        Assert.Null(producto.CodigoInterno);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_producto_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Producto(1, nombreInvalido!, CrearCategoria(), CrearUnidad()));
    }

    [Fact]
    public void No_permite_crear_producto_sin_categoria()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Producto(1, "Papel higiénico", null!, CrearUnidad()));
    }

    [Fact]
    public void No_permite_crear_producto_sin_unidad_de_medida()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Producto(1, "Papel higiénico", CrearCategoria(), null!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad());

        producto.Desactivar();
        Assert.False(producto.Activo);

        producto.Activar();
        Assert.True(producto.Activo);
    }
}
