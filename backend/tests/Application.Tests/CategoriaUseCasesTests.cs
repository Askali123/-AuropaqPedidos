using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CategoriaUseCasesTests
{
    [Fact]
    public void Crea_una_categoria_activa()
    {
        var categorias = new FakeCategoriaRepository();
        var ids = new FakeGeneradorDeIdentificadores();

        var respuesta = new CrearCategoriaUseCase(categorias, ids).Ejecutar(new CrearCategoriaRequest("Aseo", "Productos de limpieza"));

        Assert.Equal("Aseo", respuesta.Nombre);
        Assert.True(respuesta.Activo);
        Assert.NotNull(categorias.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Obtiene_una_categoria_existente()
    {
        var categorias = new FakeCategoriaRepository();
        var categoria = new Categoria(1, "Aseo");
        categorias.Agregar(categoria);

        var respuesta = new ObtenerCategoriaUseCase(categorias).Ejecutar(categoria.Id);

        Assert.Equal("Aseo", respuesta.Nombre);
    }

    [Fact]
    public void Obtener_categoria_inexistente_lanza_no_encontrado()
    {
        var categorias = new FakeCategoriaRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerCategoriaUseCase(categorias).Ejecutar(999));
    }

    [Fact]
    public void Actualiza_datos_y_puede_desactivar_una_categoria()
    {
        var categorias = new FakeCategoriaRepository();
        var categoria = new Categoria(1, "Aseo");
        categorias.Agregar(categoria);

        var respuesta = new ActualizarCategoriaUseCase(categorias).Ejecutar(
            categoria.Id, new ActualizarCategoriaRequest("Aseo y limpieza", "Nueva descripción", false));

        Assert.Equal("Aseo y limpieza", respuesta.Nombre);
        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Actualizar_categoria_inexistente_lanza_no_encontrado()
    {
        var categorias = new FakeCategoriaRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ActualizarCategoriaUseCase(categorias).Ejecutar(
            999, new ActualizarCategoriaRequest("X", null, true)));
    }

    [Fact]
    public void Actualizar_categoria_con_nombre_vacio_lanza_regla_de_negocio()
    {
        var categorias = new FakeCategoriaRepository();
        var categoria = new Categoria(1, "Aseo");
        categorias.Agregar(categoria);

        Assert.Throws<ReglaDeNegocioException>(() => new ActualizarCategoriaUseCase(categorias).Ejecutar(
            categoria.Id, new ActualizarCategoriaRequest("", null, true)));
    }
}
