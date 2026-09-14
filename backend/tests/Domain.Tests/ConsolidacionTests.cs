using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class ConsolidacionTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto(int id, string nombre) =>
        new(id, nombre, new Categoria(id, "Aseo"), new UnidadMedida(id, "UNIDAD", "Unidad"));

    // Construye una requisición APROBADA con un único detalle del producto indicado,
    // siguiendo el mismo flujo BORRADOR -> ENVIADA -> EN_REVISION -> APROBADA ya probado
    // en RequisicionFlujoTests.
    private static (Requisicion Requisicion, DetalleRequisicion Detalle) CrearRequisicionAprobada(
        int requisicionId, int empresaId, Producto producto, int cantidad, Periodo? periodo = null)
    {
        var empresa = new Empresa(empresaId, $"Empresa {empresaId}");
        var per = periodo ?? CrearPeriodo();
        var requisicion = new Requisicion(requisicionId, empresa, per, 10, Fecha);
        var detalle = requisicion.AgregarDetalle(requisicionId * 100 + 1, producto, cantidad);
        var sede = new Sede(requisicionId, empresa, $"Sede {requisicionId}");
        requisicion.AgregarDistribucion(requisicionId * 100 + 2, detalle, sede, cantidad);

        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);

        return (requisicion, detalle);
    }

    private static Consolidacion CrearConsolidacion(Periodo? periodo = null) =>
        new(1, periodo ?? CrearPeriodo(), usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);

    [Fact]
    public void Una_requisicion_aprobada_puede_participar_en_la_consolidacion()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (requisicion, detalle) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad: 30);
        var consolidacion = CrearConsolidacion();

        consolidacion.AgregarAsignacion(1, 1, requisicion, detalle, 30);

        Assert.Single(consolidacion.Detalles);
        Assert.Equal(30, consolidacion.Detalles[0].CantidadNecesaria);
    }

    [Fact]
    public void Una_requisicion_no_aprobada_no_puede_participar_en_la_consolidacion()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var empresa = new Empresa(1, "Empresa 1");
        var requisicion = new Requisicion(1, empresa, CrearPeriodo(), 10, Fecha);
        var detalle = requisicion.AgregarDetalle(1, producto, 30);
        // Se queda en BORRADOR: nunca se envía/aprueba.

        var consolidacion = CrearConsolidacion();

        Assert.Throws<ReglaDeNegocioException>(() =>
            consolidacion.AgregarAsignacion(1, 1, requisicion, detalle, 30));
    }

    [Theory]
    [InlineData(RequisicionEstado.Enviada)]
    [InlineData(RequisicionEstado.EnRevision)]
    [InlineData(RequisicionEstado.Devuelta)]
    public void Una_requisicion_en_otros_estados_distintos_de_aprobada_no_puede_participar(RequisicionEstado estado)
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var empresa = new Empresa(1, "Empresa 1");
        var requisicion = new Requisicion(1, empresa, CrearPeriodo(), 10, Fecha);
        var detalle = requisicion.AgregarDetalle(1, producto, 30);
        var sede = new Sede(1, empresa, "Sede 1");
        requisicion.AgregarDistribucion(2, detalle, sede, 30);

        requisicion.Enviar(1, Fecha);
        if (estado is RequisicionEstado.EnRevision or RequisicionEstado.Devuelta)
            requisicion.IniciarRevision(1, Fecha);
        if (estado == RequisicionEstado.Devuelta)
            requisicion.Devolver(1, Fecha, "Corregir");

        Assert.Equal(estado, requisicion.Estado);

        var consolidacion = CrearConsolidacion();

        Assert.Throws<ReglaDeNegocioException>(() =>
            consolidacion.AgregarAsignacion(1, 1, requisicion, detalle, 30));
    }

    [Fact]
    public void Dos_detalles_del_mismo_producto_de_distintas_requisiciones_se_consolidan_en_un_solo_total()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (requisicionA, detalleA) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad: 30);
        var (requisicionB, detalleB) = CrearRequisicionAprobada(2, empresaId: 2, producto, cantidad: 20);

        var consolidacion = CrearConsolidacion();
        consolidacion.AgregarAsignacion(1, 1, requisicionA, detalleA, 30);
        consolidacion.AgregarAsignacion(1, 2, requisicionB, detalleB, 20);

        Assert.Single(consolidacion.Detalles);
        Assert.Equal(50, consolidacion.Detalles[0].CantidadNecesaria);
        Assert.Equal(2, consolidacion.Detalles[0].Asignaciones.Count);
    }

    [Fact]
    public void Productos_diferentes_no_se_agrupan_en_el_mismo_detalle_de_consolidacion()
    {
        var productoA = CrearProducto(1, "Papel higiénico");
        var productoB = CrearProducto(2, "Jabón");
        var (requisicionA, detalleA) = CrearRequisicionAprobada(1, empresaId: 1, productoA, cantidad: 30);
        var (requisicionB, detalleB) = CrearRequisicionAprobada(2, empresaId: 2, productoB, cantidad: 20);

        var consolidacion = CrearConsolidacion();
        consolidacion.AgregarAsignacion(1, 1, requisicionA, detalleA, 30);
        consolidacion.AgregarAsignacion(2, 2, requisicionB, detalleB, 20);

        Assert.Equal(2, consolidacion.Detalles.Count);
        Assert.Equal(30, consolidacion.Detalles.Single(d => d.Producto == productoA).CantidadNecesaria);
        Assert.Equal(20, consolidacion.Detalles.Single(d => d.Producto == productoB).CantidadNecesaria);
    }

    [Fact]
    public void La_consolidacion_no_modifica_las_requisiciones_originales()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (requisicion, detalle) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad: 30);

        var consolidacion = CrearConsolidacion();
        consolidacion.AgregarAsignacion(1, 1, requisicion, detalle, 30);

        Assert.Equal(RequisicionEstado.Aprobada, requisicion.Estado);
        Assert.Equal(30, detalle.CantidadSolicitada);
        Assert.Single(requisicion.Detalles);
    }

    [Fact]
    public void La_asignacion_conserva_trazabilidad_hacia_el_detalle_de_requisicion_original()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (requisicionA, detalleA) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad: 30);
        var (requisicionB, detalleB) = CrearRequisicionAprobada(2, empresaId: 2, producto, cantidad: 20);

        var consolidacion = CrearConsolidacion();
        consolidacion.AgregarAsignacion(1, 1, requisicionA, detalleA, 30);
        consolidacion.AgregarAsignacion(1, 2, requisicionB, detalleB, 20);

        var asignaciones = consolidacion.Detalles[0].Asignaciones;
        Assert.Contains(asignaciones, a => a.DetalleRequisicionOrigen == detalleA && a.Cantidad == 30);
        Assert.Contains(asignaciones, a => a.DetalleRequisicionOrigen == detalleB && a.Cantidad == 20);
    }

    [Fact]
    public void No_permite_asignar_un_detalle_que_no_pertenece_a_la_requisicion_indicada()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (requisicionA, _) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad: 30);
        var (requisicionB, detalleB) = CrearRequisicionAprobada(2, empresaId: 2, producto, cantidad: 20);

        var consolidacion = CrearConsolidacion();

        // detalleB pertenece a requisicionB, no a requisicionA.
        Assert.Throws<ReglaDeNegocioException>(() =>
            consolidacion.AgregarAsignacion(1, 1, requisicionA, detalleB, 20));
    }
}
