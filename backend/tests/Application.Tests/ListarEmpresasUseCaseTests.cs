using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ListarEmpresasUseCaseTests
{
    [Fact]
    public void Devuelve_todas_las_empresas_existentes()
    {
        var empresas = new FakeEmpresaRepository();
        empresas.Agregar(new Empresa(2, "Empresa B"));
        empresas.Agregar(new Empresa(1, "Empresa A"));

        var respuesta = new ListarEmpresasUseCase(empresas).Ejecutar();

        Assert.Equal(2, respuesta.Count);
        Assert.Equal("Empresa A", respuesta[0].Nombre);
        Assert.Equal("Empresa B", respuesta[1].Nombre);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_empresas()
    {
        var respuesta = new ListarEmpresasUseCase(new FakeEmpresaRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Incluye_empresas_inactivas_sin_filtrarlas()
    {
        var empresas = new FakeEmpresaRepository();
        var empresaInactiva = new Empresa(1, "Empresa inactiva");
        empresaInactiva.Desactivar();
        empresas.Agregar(empresaInactiva);

        var respuesta = new ListarEmpresasUseCase(empresas).Ejecutar();

        var dto = Assert.Single(respuesta);
        Assert.False(dto.Activo);
    }
}
