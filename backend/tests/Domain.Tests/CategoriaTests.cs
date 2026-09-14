using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class CategoriaTests
{
    [Fact]
    public void Crear_categoria_valida_queda_activa()
    {
        var categoria = new Categoria(1, "Aseo", "Productos de limpieza");

        Assert.Equal("Aseo", categoria.Nombre);
        Assert.Equal("Productos de limpieza", categoria.Descripcion);
        Assert.True(categoria.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_categoria_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Categoria(1, nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var categoria = new Categoria(1, "Cafetería");

        categoria.Desactivar();
        Assert.False(categoria.Activo);

        categoria.Activar();
        Assert.True(categoria.Activo);
    }
}
