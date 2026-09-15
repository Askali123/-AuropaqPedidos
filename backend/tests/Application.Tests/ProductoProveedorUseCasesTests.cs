using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

// TASK-019, 02-dominio.md §11, 04-base-datos.md §15, 05-api.md §30.
public class ProductoProveedorUseCasesTests
{
    private sealed class Escenario
    {
        public FakeProductoRepository Productos { get; } = new();
        public FakeProveedorRepository Proveedores { get; } = new();
        public FakeProductoProveedorRepository Relaciones { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();

        public Producto Producto { get; } = new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));
        public Proveedor Proveedor { get; } = new(1, "Distribuidora ABC");

        public Escenario()
        {
            Productos.Agregar(Producto);
            Proveedores.Agregar(Proveedor);
        }

        public AsociarProveedorAProductoUseCase AsociarUseCase() => new(Relaciones, Productos, Proveedores, Ids);
        public ActualizarProductoProveedorUseCase ActualizarUseCase() => new(Relaciones);
        public ListarProveedoresDeProductoUseCase ListarPorProductoUseCase() => new(Productos, Relaciones);
        public ListarProductosDeProveedorUseCase ListarPorProveedorUseCase() => new(Proveedores, Relaciones);
    }

    [Fact]
    public void Asocia_un_proveedor_a_un_producto()
    {
        var escenario = new Escenario();

        var respuesta = escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id,
            new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001", "Papel doble hoja", "Aseo", "Rollo"));

        Assert.Equal(escenario.Producto.Id, respuesta.ProductoId);
        Assert.Equal(escenario.Proveedor.Id, respuesta.ProveedorId);
        Assert.Equal("ABC-001", respuesta.CodigoProveedor);
        Assert.True(respuesta.Activo);
    }

    [Fact]
    public void Asociar_con_producto_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.AsociarUseCase().Ejecutar(
            999, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001")));
    }

    [Fact]
    public void Asociar_con_proveedor_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id, new CrearProductoProveedorRequest(999, "ABC-001")));
    }

    // 04-base-datos.md §15 "Restricción": no debe duplicarse la misma relación producto/proveedor.
    [Fact]
    public void No_permite_asociar_el_mismo_proveedor_dos_veces_al_mismo_producto()
    {
        var escenario = new Escenario();
        escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001"));

        Assert.Throws<ReglaDeNegocioException>(() => escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-002")));
    }

    [Fact]
    public void Actualiza_el_codigo_y_los_datos_del_proveedor()
    {
        var escenario = new Escenario();
        var creada = escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001"));

        var respuesta = escenario.ActualizarUseCase().Ejecutar(
            escenario.Producto.Id, creada.Id,
            new ActualizarProductoProveedorRequest("ABC-002", "Nueva descripción", "Limpieza", "Caja", Activo: false));

        Assert.Equal("ABC-002", respuesta.CodigoProveedor);
        Assert.Equal("Nueva descripción", respuesta.DescripcionProveedor);
        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Actualizar_relacion_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.ActualizarUseCase().Ejecutar(
            escenario.Producto.Id, 999, new ActualizarProductoProveedorRequest("ABC-002", null, null, null, true)));
    }

    // TASK-051: el productoId de la ruta debe corresponder realmente a la relación.
    [Fact]
    public void Actualizar_con_productoId_que_no_corresponde_a_la_relacion_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var otroProducto = new Producto(2, "Jabón", new Categoria(2, "Aseo"), new UnidadMedida(2, "UNIDAD", "Unidad"));
        escenario.Productos.Agregar(otroProducto);
        var creada = escenario.AsociarUseCase().Ejecutar(
            escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001"));

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.ActualizarUseCase().Ejecutar(
            otroProducto.Id, creada.Id, new ActualizarProductoProveedorRequest("ABC-002", null, null, null, true)));
    }

    [Fact]
    public void Lista_los_proveedores_de_un_producto()
    {
        var escenario = new Escenario();
        var otroProveedor = new Proveedor(2, "Distribuidora XYZ");
        escenario.Proveedores.Agregar(otroProveedor);
        escenario.AsociarUseCase().Ejecutar(escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001"));
        escenario.AsociarUseCase().Ejecutar(escenario.Producto.Id, new CrearProductoProveedorRequest(otroProveedor.Id, "XYZ-001"));

        var respuesta = escenario.ListarPorProductoUseCase().Ejecutar(escenario.Producto.Id);

        Assert.Equal(2, respuesta.Count);
    }

    [Fact]
    public void Listar_proveedores_de_producto_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.ListarPorProductoUseCase().Ejecutar(999));
    }

    [Fact]
    public void Lista_los_productos_de_un_proveedor()
    {
        var escenario = new Escenario();
        var otroProducto = new Producto(2, "Jabón", new Categoria(2, "Aseo"), new UnidadMedida(2, "UNIDAD", "Unidad"));
        escenario.Productos.Agregar(otroProducto);
        escenario.AsociarUseCase().Ejecutar(escenario.Producto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-001"));
        escenario.AsociarUseCase().Ejecutar(otroProducto.Id, new CrearProductoProveedorRequest(escenario.Proveedor.Id, "ABC-002"));

        var respuesta = escenario.ListarPorProveedorUseCase().Ejecutar(escenario.Proveedor.Id);

        Assert.Equal(2, respuesta.Count);
    }

    [Fact]
    public void Listar_productos_de_proveedor_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.ListarPorProveedorUseCase().Ejecutar(999));
    }
}
