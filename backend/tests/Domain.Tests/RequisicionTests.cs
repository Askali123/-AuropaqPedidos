using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class RequisicionTests
{
    private static Empresa CrearEmpresa() => new(1, "AUROTECH");

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto() =>
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    private static Requisicion CrearRequisicion() =>
        new(1, CrearEmpresa(), CrearPeriodo(), usuarioCreacionId: 10, fechaCreacion: new DateTime(2026, 9, 1));

    [Fact]
    public void Crear_requisicion_inicia_en_borrador_y_sin_detalles()
    {
        var requisicion = CrearRequisicion();

        Assert.Equal(RequisicionEstado.Borrador, requisicion.Estado);
        Assert.Empty(requisicion.Detalles);
        Assert.True(requisicion.EsEditable);
    }

    [Fact]
    public void No_permite_crear_requisicion_sin_empresa()
    {
        Assert.Throws<ReglaDeNegocioException>(() =>
            new Requisicion(1, null!, CrearPeriodo(), 10, new DateTime(2026, 9, 1)));
    }

    [Fact]
    public void No_permite_crear_requisicion_sin_periodo()
    {
        Assert.Throws<ReglaDeNegocioException>(() =>
            new Requisicion(1, CrearEmpresa(), null!, 10, new DateTime(2026, 9, 1)));
    }

    [Fact]
    public void Agregar_detalle_valido_lo_incluye_en_la_lista()
    {
        var requisicion = CrearRequisicion();
        var producto = CrearProducto();

        var detalle = requisicion.AgregarDetalle(1, producto, 100, "Urgente");

        Assert.Single(requisicion.Detalles);
        Assert.Equal(producto, detalle.Producto);
        Assert.Equal(100, detalle.CantidadSolicitada);
        Assert.Equal("Urgente", detalle.Observacion);
    }

    [Fact]
    public void No_permite_agregar_detalle_con_cantidad_invalida()
    {
        var requisicion = CrearRequisicion();

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.AgregarDetalle(1, CrearProducto(), 0));
    }

    [Fact]
    public void Modificar_cantidad_de_detalle_actualiza_el_valor()
    {
        var requisicion = CrearRequisicion();
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), 100);

        requisicion.ModificarCantidadDetalle(detalle, 150);

        Assert.Equal(150, detalle.CantidadSolicitada);
    }

    [Fact]
    public void No_permite_reducir_cantidad_por_debajo_de_lo_ya_distribuido()
    {
        var requisicion = CrearRequisicion();
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), 100);
        var sede = new Sede(1, requisicion.Empresa, "Bogotá");
        requisicion.AgregarDistribucion(1, detalle, sede, 60);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.ModificarCantidadDetalle(detalle, 50));
    }

    [Fact]
    public void Eliminar_detalle_lo_remueve_de_la_lista()
    {
        var requisicion = CrearRequisicion();
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), 100);

        requisicion.EliminarDetalle(detalle);

        Assert.Empty(requisicion.Detalles);
    }

    [Fact]
    public void No_permite_operar_sobre_un_detalle_que_no_pertenece_a_la_requisicion()
    {
        var requisicion = CrearRequisicion();
        var otraRequisicion = CrearRequisicion();
        var detalleAjeno = otraRequisicion.AgregarDetalle(1, CrearProducto(), 100);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.EliminarDetalle(detalleAjeno));
    }

    [Fact]
    public void No_permite_agregar_detalle_cuando_no_esta_editable()
    {
        var requisicion = CrearRequisicion();
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), 100);
        var sede = new Sede(1, requisicion.Empresa, "Bogotá");
        requisicion.AgregarDistribucion(1, detalle, sede, 100);
        requisicion.Enviar(usuarioId: 1, fechaEnvio: new DateTime(2026, 9, 2));

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.AgregarDetalle(2, CrearProducto(), 50));
    }
}
