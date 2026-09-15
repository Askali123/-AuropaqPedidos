using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class EmpresaTests
{
    [Fact]
    public void Crear_empresa_valida_queda_activa()
    {
        var empresa = new Empresa(1, "AUROTECH", "900123456-1");

        Assert.Equal(1, empresa.Id);
        Assert.Equal("AUROTECH", empresa.Nombre);
        Assert.True(empresa.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_empresa_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Empresa(1, nombreInvalido!));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado()
    {
        var empresa = new Empresa(1, "FAVIPAQ");

        empresa.Desactivar();
        Assert.False(empresa.Activo);

        empresa.Activar();
        Assert.True(empresa.Activo);
    }

    [Fact]
    public void ActualizarDatos_cambia_nombre_y_nit_sin_afectar_el_estado()
    {
        var empresa = new Empresa(1, "AUROTECH", "900000000-1");
        empresa.Desactivar();

        empresa.ActualizarDatos("AUROTECH S.A.S.", "900111111-2");

        Assert.Equal("AUROTECH S.A.S.", empresa.Nombre);
        Assert.Equal("900111111-2", empresa.Nit);
        Assert.False(empresa.Activo);
    }

    [Fact]
    public void ActualizarDatos_permite_dejar_el_nit_en_null()
    {
        var empresa = new Empresa(1, "AUROTECH", "900000000-1");

        empresa.ActualizarDatos("AUROTECH", null);

        Assert.Null(empresa.Nit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarDatos_no_permite_nombre_vacio(string? nombreInvalido)
    {
        var empresa = new Empresa(1, "AUROTECH");

        Assert.Throws<ReglaDeNegocioException>(() => empresa.ActualizarDatos(nombreInvalido!, null));
    }
}
