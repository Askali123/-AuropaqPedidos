using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class DistribucionRequisicionTests
{
    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto() =>
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    private static (Requisicion requisicion, DetalleRequisicion detalle) CrearRequisicionConDetalle(int cantidad = 100)
    {
        var empresa = new Empresa(1, "AUROTECH");
        var requisicion = new Requisicion(1, empresa, CrearPeriodo(), 10, new DateTime(2026, 9, 1));
        var detalle = requisicion.AgregarDetalle(1, CrearProducto(), cantidad);
        return (requisicion, detalle);
    }

    [Fact]
    public void Distribuir_dentro_de_la_cantidad_solicitada_es_valido()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var bogota = new Sede(1, requisicion.Empresa, "Bogotá");
        var medellin = new Sede(2, requisicion.Empresa, "Medellín");

        requisicion.AgregarDistribucion(1, detalle, bogota, 60);
        requisicion.AgregarDistribucion(2, detalle, medellin, 40);

        Assert.Equal(100, detalle.CantidadDistribuida);
        Assert.True(detalle.DistribucionCompleta);
    }

    [Fact]
    public void No_permite_distribuir_mas_de_la_cantidad_solicitada()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var bogota = new Sede(1, requisicion.Empresa, "Bogotá");
        requisicion.AgregarDistribucion(1, detalle, bogota, 60);

        var medellin = new Sede(2, requisicion.Empresa, "Medellín");

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.AgregarDistribucion(2, detalle, medellin, 41));
    }

    [Fact]
    public void No_permite_distribuir_a_una_sede_de_otra_empresa()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var otraEmpresa = new Empresa(2, "FAVIPAQ");
        var sedeAjena = new Sede(1, otraEmpresa, "Cali");

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.AgregarDistribucion(1, detalle, sedeAjena, 50));
    }

    [Fact]
    public void No_permite_distribucion_con_cantidad_invalida()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var bogota = new Sede(1, requisicion.Empresa, "Bogotá");

        Assert.Throws<ReglaDeNegocioException>(() => requisicion.AgregarDistribucion(1, detalle, bogota, 0));
    }

    [Fact]
    public void Modificar_distribucion_respeta_la_suma_maxima()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var bogota = new Sede(1, requisicion.Empresa, "Bogotá");
        var medellin = new Sede(2, requisicion.Empresa, "Medellín");
        requisicion.AgregarDistribucion(1, detalle, bogota, 60);
        var distribucionMedellin = requisicion.AgregarDistribucion(2, detalle, medellin, 40);

        Assert.Throws<ReglaDeNegocioException>(() =>
            requisicion.ModificarDistribucion(detalle, distribucionMedellin, 41));
    }

    [Fact]
    public void Eliminar_distribucion_permite_redistribuir()
    {
        var (requisicion, detalle) = CrearRequisicionConDetalle(100);
        var bogota = new Sede(1, requisicion.Empresa, "Bogotá");
        var distribucion = requisicion.AgregarDistribucion(1, detalle, bogota, 60);

        requisicion.EliminarDistribucion(detalle, distribucion);

        Assert.Empty(detalle.Distribuciones);
        Assert.Equal(0, detalle.CantidadDistribuida);
    }
}
