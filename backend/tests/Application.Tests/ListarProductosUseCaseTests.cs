using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ListarProductosUseCaseTests
{
    [Fact]
    public void Devuelve_todos_los_productos_existentes_con_su_unidad_de_medida()
    {
        var productos = new FakeProductoRepository();
        var categoria = new Categoria(1, "Aseo");
        var unidad = new UnidadMedida(1, "UNIDAD", "Unidad");
        productos.Agregar(new Producto(2, "Producto B", categoria, unidad));
        productos.Agregar(new Producto(1, "Producto A", categoria, unidad, codigoInterno: "COD-A"));

        var respuesta = new ListarProductosUseCase(productos).Ejecutar();

        Assert.Equal(2, respuesta.Count);
        Assert.Equal("Producto A", respuesta[0].Nombre);
        Assert.Equal("COD-A", respuesta[0].CodigoInterno);
        Assert.Equal("UNIDAD", respuesta[0].UnidadMedidaCodigo);
        Assert.Equal("Unidad", respuesta[0].UnidadMedidaNombre);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_productos()
    {
        var respuesta = new ListarProductosUseCase(new FakeProductoRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Incluye_productos_inactivos_sin_filtrarlos()
    {
        var productos = new FakeProductoRepository();
        var categoria = new Categoria(1, "Aseo");
        var unidad = new UnidadMedida(1, "UNIDAD", "Unidad");
        var productoInactivo = new Producto(1, "Producto inactivo", categoria, unidad);
        productoInactivo.Desactivar();
        productos.Agregar(productoInactivo);

        var respuesta = new ListarProductosUseCase(productos).Ejecutar();

        var dto = Assert.Single(respuesta);
        Assert.False(dto.Activo);
    }
}
