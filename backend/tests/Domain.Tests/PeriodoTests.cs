using System.Globalization;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class PeriodoTests
{
    private static Periodo CrearPeriodoValido() => new(
        id: 1,
        anio: 2026,
        mes: 9,
        fechaInicio: new DateTime(2026, 9, 1),
        fechaFin: new DateTime(2026, 9, 30),
        fechaInicioSolicitud: new DateTime(2026, 9, 1),
        fechaFinSolicitud: new DateTime(2026, 9, 3),
        estado: "ABIERTO");

    [Fact]
    public void Crear_periodo_valido_conserva_sus_datos()
    {
        var periodo = CrearPeriodoValido();

        Assert.Equal(2026, periodo.Anio);
        Assert.Equal(9, periodo.Mes);
        Assert.Equal("ABIERTO", periodo.Estado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void No_permite_mes_fuera_de_rango(int mesInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Periodo(
            1, 2026, mesInvalido,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO"));
    }

    [Fact]
    public void No_permite_fecha_fin_de_periodo_anterior_a_fecha_inicio()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Periodo(
            1, 2026, 9,
            new DateTime(2026, 9, 30), new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO"));
    }

    [Fact]
    public void No_permite_ventana_de_solicitud_invertida()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Periodo(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 3), new DateTime(2026, 9, 1),
            "ABIERTO"));
    }

    [Theory]
    [InlineData("2026-09-02", true)]
    [InlineData("2026-09-04", false)]
    public void EstaDentroDeVentanaDeSolicitud_evalua_correctamente(string fechaTexto, bool esperado)
    {
        var periodo = CrearPeriodoValido();
        var fecha = DateTime.ParseExact(fechaTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        Assert.Equal(esperado, periodo.EstaDentroDeVentanaDeSolicitud(fecha));
    }
}
