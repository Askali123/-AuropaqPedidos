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

    [Fact]
    public void ActualizarDatos_cambia_datos_categoria_y_unidad_sin_afectar_el_estado()
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad(), codigoInterno: "PAP-001");
        producto.Desactivar();
        var nuevaCategoria = new Categoria(2, "Aseo");
        var nuevaUnidad = new UnidadMedida(2, "PAQUETE", "Paquete");

        producto.ActualizarDatos("Papel higiénico premium", nuevaCategoria, nuevaUnidad, "PAP-002", "Nueva descripción");

        Assert.Equal("Papel higiénico premium", producto.Nombre);
        Assert.Equal(nuevaCategoria, producto.Categoria);
        Assert.Equal(nuevaUnidad, producto.UnidadMedida);
        Assert.Equal("PAP-002", producto.CodigoInterno);
        Assert.False(producto.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_nombre_vacio(string? nombreInvalido)
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad());

        Assert.Throws<ReglaDeNegocioException>(() => producto.ActualizarDatos(nombreInvalido!, CrearCategoria(), CrearUnidad()));
    }

    [Fact]
    public void ActualizarDatos_no_permite_categoria_nula()
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad());

        Assert.Throws<ReglaDeNegocioException>(() => producto.ActualizarDatos("Papel higiénico", null!, CrearUnidad()));
    }

    [Fact]
    public void ActualizarDatos_no_permite_unidad_de_medida_nula()
    {
        var producto = new Producto(1, "Papel higiénico", CrearCategoria(), CrearUnidad());

        Assert.Throws<ReglaDeNegocioException>(() => producto.ActualizarDatos("Papel higiénico", CrearCategoria(), null!));
    }
}
