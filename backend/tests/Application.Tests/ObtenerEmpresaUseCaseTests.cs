using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ObtenerEmpresaUseCaseTests
{
    [Fact]
    public void Devuelve_la_empresa_solicitada()
    {
        var empresas = new FakeEmpresaRepository();
        var empresa = new Empresa(1, "AUROTECH");
        empresas.Agregar(empresa);

        var respuesta = new ObtenerEmpresaUseCase(empresas).Ejecutar(empresa.Id);

        Assert.Equal(empresa.Id, respuesta.Id);
        Assert.Equal("AUROTECH", respuesta.Nombre);
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_empresa_no_existe()
    {
        var empresas = new FakeEmpresaRepository();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerEmpresaUseCase(empresas).Ejecutar(999));
    }
}
