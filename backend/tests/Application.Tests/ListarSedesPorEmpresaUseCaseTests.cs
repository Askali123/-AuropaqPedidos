using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ListarSedesPorEmpresaUseCaseTests
{
    private static (FakeEmpresaRepository Empresas, FakeSedeRepository Sedes, Empresa Empresa) Escenario()
    {
        var empresas = new FakeEmpresaRepository();
        var sedes = new FakeSedeRepository();
        var empresa = new Empresa(1, "Empresa 1");
        empresas.Agregar(empresa);
        return (empresas, sedes, empresa);
    }

    [Fact]
    public void Devuelve_las_sedes_de_la_empresa_indicada()
    {
        var (empresas, sedes, empresa) = Escenario();
        var otraEmpresa = new Empresa(2, "Empresa 2");
        empresas.Agregar(otraEmpresa);
        sedes.Agregar(new Sede(2, empresa, "Sede B"));
        sedes.Agregar(new Sede(1, empresa, "Sede A"));
        sedes.Agregar(new Sede(3, otraEmpresa, "Sede de otra empresa"));

        var respuesta = new ListarSedesPorEmpresaUseCase(empresas, sedes).Ejecutar(empresa.Id);

        Assert.Equal(2, respuesta.Count);
        Assert.Equal("Sede A", respuesta[0].Nombre);
        Assert.Equal("Sede B", respuesta[1].Nombre);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_la_empresa_no_tiene_sedes()
    {
        var (empresas, sedes, empresa) = Escenario();

        var respuesta = new ListarSedesPorEmpresaUseCase(empresas, sedes).Ejecutar(empresa.Id);

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_empresa_no_existe()
    {
        var (empresas, sedes, _) = Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            new ListarSedesPorEmpresaUseCase(empresas, sedes).Ejecutar(999));
    }

    [Fact]
    public void Incluye_sedes_inactivas_sin_filtrarlas()
    {
        var (empresas, sedes, empresa) = Escenario();
        var sedeInactiva = new Sede(1, empresa, "Sede inactiva");
        sedeInactiva.Desactivar();
        sedes.Agregar(sedeInactiva);

        var respuesta = new ListarSedesPorEmpresaUseCase(empresas, sedes).Ejecutar(empresa.Id);

        var dto = Assert.Single(respuesta);
        Assert.False(dto.Activo);
    }
}
