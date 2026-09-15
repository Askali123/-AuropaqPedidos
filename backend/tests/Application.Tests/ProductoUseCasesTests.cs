using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ProductoUseCasesTests
{
    private sealed class Escenario
    {
        public FakeProductoRepository Productos { get; } = new();
        public FakeCategoriaRepository Categorias { get; } = new();
        public FakeUnidadMedidaRepository UnidadesMedida { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public Categoria Categoria { get; } = new(1, "Papelería");
        public UnidadMedida UnidadMedida { get; } = new(1, "UNIDAD", "Unidad");

        public Escenario()
        {
            Categorias.Agregar(Categoria);
            UnidadesMedida.Agregar(UnidadMedida);
        }

        public CrearProductoUseCase CrearUseCase() => new(Productos, Categorias, UnidadesMedida, Ids);

        public ActualizarProductoUseCase ActualizarUseCase() => new(Productos, Categorias, UnidadesMedida);
    }

    [Fact]
    public void Crea_un_producto_activo_asociado_a_categoria_y_unidad()
    {
        var escenario = new Escenario();

        var respuesta = escenario.CrearUseCase().Ejecutar(
            new CrearProductoRequest("Abrasivo Regular", escenario.Categoria.Id, escenario.UnidadMedida.Id, "1281", "Descripción"));

        Assert.Equal("Abrasivo Regular", respuesta.Nombre);
        Assert.Equal(escenario.Categoria.Id, respuesta.CategoriaId);
        Assert.Equal(escenario.UnidadMedida.Id, respuesta.UnidadMedidaId);
        Assert.True(respuesta.Activo);
        Assert.NotNull(escenario.Productos.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Crear_producto_con_categoria_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.CrearUseCase().Ejecutar(
            new CrearProductoRequest("Producto X", 999, escenario.UnidadMedida.Id)));
    }

    [Fact]
    public void Crear_producto_con_unidad_de_medida_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.CrearUseCase().Ejecutar(
            new CrearProductoRequest("Producto X", escenario.Categoria.Id, 999)));
    }

    [Fact]
    public void Obtiene_un_producto_existente()
    {
        var escenario = new Escenario();
        var producto = new Producto(1, "Papel higiénico", escenario.Categoria, escenario.UnidadMedida);
        escenario.Productos.Agregar(producto);

        var respuesta = new ObtenerProductoUseCase(escenario.Productos).Ejecutar(producto.Id);

        Assert.Equal("Papel higiénico", respuesta.Nombre);
    }

    [Fact]
    public void Obtener_producto_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerProductoUseCase(escenario.Productos).Ejecutar(999));
    }

    [Fact]
    public void Actualiza_datos_y_puede_desactivar_un_producto()
    {
        var escenario = new Escenario();
        var producto = new Producto(1, "Papel higiénico", escenario.Categoria, escenario.UnidadMedida);
        escenario.Productos.Agregar(producto);
        var nuevaCategoria = new Categoria(2, "Aseo");
        escenario.Categorias.Agregar(nuevaCategoria);

        var respuesta = escenario.ActualizarUseCase().Ejecutar(
            producto.Id,
            new ActualizarProductoRequest("Papel higiénico premium", nuevaCategoria.Id, escenario.UnidadMedida.Id, "PAP-002", "Nueva descripción", false));

        Assert.Equal("Papel higiénico premium", respuesta.Nombre);
        Assert.Equal(nuevaCategoria.Id, respuesta.CategoriaId);
        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Actualizar_producto_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.ActualizarUseCase().Ejecutar(
            999, new ActualizarProductoRequest("X", escenario.Categoria.Id, escenario.UnidadMedida.Id, null, null, true)));
    }
}
