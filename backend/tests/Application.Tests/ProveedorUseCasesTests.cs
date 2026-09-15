using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class ProveedorUseCasesTests
{
    [Fact]
    public void Crea_un_proveedor_activo()
    {
        var proveedores = new FakeProveedorRepository();
        var ids = new FakeGeneradorDeIdentificadores();

        var respuesta = new CrearProveedorUseCase(proveedores, ids).Ejecutar(
            new CrearProveedorRequest("Distribuidora ABC", "900000000-1"));

        Assert.Equal("Distribuidora ABC", respuesta.Nombre);
        Assert.True(respuesta.Activo);
        Assert.NotNull(proveedores.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Obtiene_un_proveedor_existente()
    {
        var proveedores = new FakeProveedorRepository();
        var proveedor = new Proveedor(1, "Distribuidora ABC");
        proveedores.Agregar(proveedor);

        var respuesta = new ObtenerProveedorUseCase(proveedores).Ejecutar(proveedor.Id);

        Assert.Equal("Distribuidora ABC", respuesta.Nombre);
    }

    [Fact]
    public void Obtener_proveedor_inexistente_lanza_no_encontrado()
    {
        var proveedores = new FakeProveedorRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerProveedorUseCase(proveedores).Ejecutar(999));
    }

    [Fact]
    public void Actualiza_datos_y_puede_desactivar_un_proveedor()
    {
        var proveedores = new FakeProveedorRepository();
        var proveedor = new Proveedor(1, "Distribuidora ABC");
        proveedores.Agregar(proveedor);

        var respuesta = new ActualizarProveedorUseCase(proveedores).Ejecutar(
            proveedor.Id, new ActualizarProveedorRequest("Distribuidora ABC S.A.S.", "900111111-2", null, null, null, false));

        Assert.Equal("Distribuidora ABC S.A.S.", respuesta.Nombre);
        Assert.False(respuesta.Activo);
    }

    [Fact]
    public void Actualizar_proveedor_inexistente_lanza_no_encontrado()
    {
        var proveedores = new FakeProveedorRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ActualizarProveedorUseCase(proveedores).Ejecutar(
            999, new ActualizarProveedorRequest("X", null, null, null, null, true)));
    }

    [Fact]
    public void Actualizar_proveedor_con_nombre_vacio_lanza_regla_de_negocio()
    {
        var proveedores = new FakeProveedorRepository();
        var proveedor = new Proveedor(1, "Distribuidora ABC");
        proveedores.Agregar(proveedor);

        Assert.Throws<ReglaDeNegocioException>(() => new ActualizarProveedorUseCase(proveedores).Ejecutar(
            proveedor.Id, new ActualizarProveedorRequest("", null, null, null, null, true)));
    }
}
