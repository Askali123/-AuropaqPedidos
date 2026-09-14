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
}
