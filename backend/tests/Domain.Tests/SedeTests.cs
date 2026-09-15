using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class SedeTests
{
    private static Empresa CrearEmpresa() => new(1, "COURIERBOX");

    [Fact]
    public void Crear_sede_valida_queda_activa_y_asociada_a_su_empresa()
    {
        var empresa = CrearEmpresa();

        var sede = new Sede(1, empresa, "Bogotá", direccion: "Calle 1 # 2-3", ciudad: "Bogotá");

        Assert.Equal(empresa, sede.Empresa);
        Assert.Equal("Bogotá", sede.Nombre);
        Assert.True(sede.Activo);
    }

    [Fact]
    public void No_permite_crear_sede_sin_empresa()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Sede(1, null!, "Bogotá"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_sede_sin_nombre(string? nombreInvalido)
    {
        var empresa = CrearEmpresa();

        Assert.Throws<ReglaDeNegocioException>(() => new Sede(1, empresa, nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var sede = new Sede(1, CrearEmpresa(), "Medellín");

        sede.Desactivar();
        Assert.False(sede.Activo);

        sede.Activar();
        Assert.True(sede.Activo);
    }

    [Fact]
    public void ActualizarDatos_cambia_los_datos_sin_afectar_la_empresa_ni_el_estado()
    {
        var empresa = CrearEmpresa();
        var sede = new Sede(1, empresa, "Bogotá", direccion: "Calle 1", ciudad: "Bogotá");
        sede.Desactivar();

        sede.ActualizarDatos("Bogotá Norte", "Calle 100", "Bogotá", "Bogotá D.C.", "6011234567", "Recepción");

        Assert.Equal("Bogotá Norte", sede.Nombre);
        Assert.Equal("Calle 100", sede.Direccion);
        Assert.Equal("Bogotá D.C.", sede.Departamento);
        Assert.Equal("6011234567", sede.Telefono);
        Assert.Equal("Recepción", sede.Contacto);
        Assert.Equal(empresa, sede.Empresa);
        Assert.False(sede.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_nombre_vacio(string? nombreInvalido)
    {
        var sede = new Sede(1, CrearEmpresa(), "Bogotá");

        Assert.Throws<ReglaDeNegocioException>(() => sede.ActualizarDatos(nombreInvalido!));
    }
}
