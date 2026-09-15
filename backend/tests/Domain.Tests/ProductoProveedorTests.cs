using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

// TASK-019, 02-dominio.md §11, 04-base-datos.md §15.
public class ProductoProveedorTests
{
    private static Producto CrearProducto() =>
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    private static Proveedor CrearProveedor() => new(1, "Distribuidora ABC");

    [Fact]
    public void Crea_una_relacion_valida_y_queda_activa()
    {
        var producto = CrearProducto();
        var proveedor = CrearProveedor();

        var relacion = new ProductoProveedor(1, producto, proveedor, "ABC-001", "Papel higiénico doble hoja", "Aseo", "Rollo");

        Assert.Equal(producto, relacion.Producto);
        Assert.Equal(proveedor, relacion.Proveedor);
        Assert.Equal("ABC-001", relacion.CodigoProveedor);
        Assert.Equal("Papel higiénico doble hoja", relacion.DescripcionProveedor);
        Assert.Equal("Aseo", relacion.CategoriaProveedor);
        Assert.Equal("Rollo", relacion.UnidadProveedor);
        Assert.True(relacion.Activo);
    }

    [Fact]
    public void No_permite_crear_sin_producto()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new ProductoProveedor(1, null!, CrearProveedor(), "ABC-001"));
    }

    [Fact]
    public void No_permite_crear_sin_proveedor()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new ProductoProveedor(1, CrearProducto(), null!, "ABC-001"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_codigo_de_proveedor_vacio(string? codigoInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new ProductoProveedor(1, CrearProducto(), CrearProveedor(), codigoInvalido!));
    }

    [Fact]
    public void Actualizar_datos_cambia_el_codigo_y_la_informacion_del_proveedor()
    {
        var relacion = new ProductoProveedor(1, CrearProducto(), CrearProveedor(), "ABC-001");

        relacion.ActualizarDatos("ABC-002", "Nueva descripción", "Limpieza", "Caja");

        Assert.Equal("ABC-002", relacion.CodigoProveedor);
        Assert.Equal("Nueva descripción", relacion.DescripcionProveedor);
        Assert.Equal("Limpieza", relacion.CategoriaProveedor);
        Assert.Equal("Caja", relacion.UnidadProveedor);
    }

    [Fact]
    public void No_permite_actualizar_con_codigo_vacio()
    {
        var relacion = new ProductoProveedor(1, CrearProducto(), CrearProveedor(), "ABC-001");

        Assert.Throws<ReglaDeNegocioException>(() => relacion.ActualizarDatos(""));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var relacion = new ProductoProveedor(1, CrearProducto(), CrearProveedor(), "ABC-001");

        relacion.Desactivar();
        Assert.False(relacion.Activo);

        relacion.Activar();
        Assert.True(relacion.Activo);
    }
}
