using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class RequisicionFlujoTests
{
    private static readonly DateTime FechaCreacion = new(2026, 9, 1);
    private static readonly DateTime DentroDeVentana = new(2026, 9, 2);
    private static readonly DateTime FueraDeVentana = new(2026, 9, 10);

    private static Empresa CrearEmpresa(bool activa = true)
    {
        var empresa = new Empresa(1, "AUROTECH");
        if (!activa) empresa.Desactivar();
        return empresa;
    }

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto() =>
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    private static Requisicion CrearRequisicionListaParaEnviar(
        Empresa? empresa = null, Periodo? periodo = null, int cantidad = 100, bool distribuirCompleto = true)
    {
        var emp = empresa ?? CrearEmpresa();
        var requisicion = new Requisicion(1, emp, periodo ?? CrearPeriodo(), 10, FechaCreacion);
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), cantidad);

        if (distribuirCompleto)
        {
            var sede = new Sede(1, emp, "Bogotá");
            requisicion.AgregarDistribucion(1, detalle, sede, cantidad);
        }

        return requisicion;
    }

    [Fact]
    public void Enviar_requisicion_valida_cambia_estado_y_registra_historial()
    {
        var requisicion = CrearRequisicionListaParaEnviar();

        requisicion.Enviar(usuarioId: 1, fechaEnvio: DentroDeVentana);

        Assert.Equal(RequisicionEstado.Enviada, requisicion.Estado);
        Assert.Equal(DentroDeVentana, requisicion.FechaEnvio);
        Assert.Single(requisicion.Historial);
        Assert.Equal(RequisicionEstado.Borrador, requisicion.Historial[0].EstadoAnterior);
        Assert.Equal(RequisicionEstado.Enviada, requisicion.Historial[0].EstadoNuevo);
    }

    [Fact]
    public void No_permite_enviar_sin_detalles()
    {
        var requisicion = new Requisicion(1, CrearEmpresa(), CrearPeriodo(), 10, FechaCreacion);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Enviar(1, DentroDeVentana));
    }

    [Fact]
    public void No_permite_enviar_con_distribucion_incompleta()
    {
        var requisicion = CrearRequisicionListaParaEnviar(distribuirCompleto: false);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Enviar(1, DentroDeVentana));
    }

    [Fact]
    public void No_permite_enviar_fuera_de_la_ventana_del_periodo()
    {
        var requisicion = CrearRequisicionListaParaEnviar();

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Enviar(1, FueraDeVentana));
    }

    [Fact]
    public void No_permite_enviar_con_empresa_inactiva()
    {
        var empresa = CrearEmpresa(activa: false);
        var requisicion = CrearRequisicionListaParaEnviar(empresa: empresa);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Enviar(1, DentroDeVentana));
    }

    [Fact]
    public void No_permite_enviar_dos_veces_consecutivas()
    {
        var requisicion = CrearRequisicionListaParaEnviar();
        requisicion.Enviar(1, DentroDeVentana);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Enviar(1, DentroDeVentana));
    }

    [Fact]
    public void IniciarRevision_solo_es_valido_desde_enviada()
    {
        var requisicion = CrearRequisicionListaParaEnviar();

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.IniciarRevision(1, DentroDeVentana));

        requisicion.Enviar(1, DentroDeVentana);
        requisicion.IniciarRevision(1, DentroDeVentana);

        Assert.Equal(RequisicionEstado.EnRevision, requisicion.Estado);
    }

    [Fact]
    public void Aprobar_requiere_estado_en_revision()
    {
        var requisicion = CrearRequisicionListaParaEnviar();
        requisicion.Enviar(1, DentroDeVentana);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Aprobar(2, DentroDeVentana));

        requisicion.IniciarRevision(1, DentroDeVentana);
        requisicion.Aprobar(2, DentroDeVentana, "Cumple los requisitos");

        Assert.Equal(RequisicionEstado.Aprobada, requisicion.Estado);
        Assert.False(requisicion.EsEditable);
    }

    [Fact]
    public void Devolver_requiere_motivo()
    {
        var requisicion = CrearRequisicionListaParaEnviar();
        requisicion.Enviar(1, DentroDeVentana);
        requisicion.IniciarRevision(1, DentroDeVentana);

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.Devolver(2, DentroDeVentana, motivo: ""));
    }

    [Fact]
    public void Devolver_cambia_estado_y_permite_corregir_y_reenviar()
    {
        var requisicion = CrearRequisicionListaParaEnviar();
        requisicion.Enviar(1, DentroDeVentana);
        requisicion.IniciarRevision(1, DentroDeVentana);

        requisicion.Devolver(2, DentroDeVentana, "Cantidad incorrecta del producto X");

        Assert.Equal(RequisicionEstado.Devuelta, requisicion.Estado);
        Assert.True(requisicion.EsEditable);

        // corrección: mientras está DEVUELTA puede modificarse y redistribuirse
        var detalle = requisicion.Detalles[0];
        requisicion.ModificarCantidadDetalle(detalle, 120);
        var medellin = new Sede(2, requisicion.Empresa, "Medellín");
        requisicion.AgregarDistribucion(2, detalle, medellin, 20);

        // reenvío
        requisicion.Enviar(usuarioId: 1, fechaEnvio: DentroDeVentana.AddHours(1));

        Assert.Equal(RequisicionEstado.Enviada, requisicion.Estado);
    }

    [Fact]
    public void Historial_conserva_todas_las_transiciones_sin_perder_las_anteriores()
    {
        var requisicion = CrearRequisicionListaParaEnviar();
        requisicion.Enviar(1, DentroDeVentana);
        requisicion.IniciarRevision(1, DentroDeVentana);
        requisicion.Devolver(2, DentroDeVentana, "Corregir cantidad");

        Assert.Equal(3, requisicion.Historial.Count);
        Assert.Equal(RequisicionEstado.Borrador, requisicion.Historial[0].EstadoAnterior);
        Assert.Equal(RequisicionEstado.Enviada, requisicion.Historial[1].EstadoAnterior);
        Assert.Equal(RequisicionEstado.EnRevision, requisicion.Historial[2].EstadoAnterior);
        Assert.Equal(RequisicionEstado.Devuelta, requisicion.Historial[2].EstadoNuevo);
        Assert.Equal("Corregir cantidad", requisicion.Historial[2].Comentario);
    }
}
