using Application.Tests.Fakes;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// TASK-050. Alcance por empresa: Usuario.Empresa == Requisicion.Empresa.
public class UsuarioTieneAlcanceSobreRequisicionUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private static Requisicion CrearRequisicion(int id, Empresa empresa) =>
        new(id, empresa, new Periodo(
            id, 2026, 1, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31),
            new DateTime(2026, 1, 1), new DateTime(2100, 1, 1), "ABIERTO"),
            usuarioCreacionId: 1, fechaCreacion: Fecha);

    [Fact]
    public void Usuario_de_la_misma_empresa_esta_dentro_del_alcance()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var requisicion = CrearRequisicion(1, empresa);

        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);
        var requisiciones = new FakeRequisicionRepository();
        requisiciones.Guardar(requisicion);

        var useCase = new UsuarioTieneAlcanceSobreRequisicionUseCase(usuarios, requisiciones);

        Assert.True(useCase.Ejecutar(usuario.Id, requisicion.Id));
    }

    [Fact]
    public void Usuario_de_otra_empresa_esta_fuera_del_alcance()
    {
        var empresaA = new Empresa(1, "AUROTECH");
        var empresaB = new Empresa(2, "COURIERBOX");
        var usuario = new Usuario(1, empresaA, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var requisicionDeEmpresaB = CrearRequisicion(1, empresaB);

        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);
        var requisiciones = new FakeRequisicionRepository();
        requisiciones.Guardar(requisicionDeEmpresaB);

        var useCase = new UsuarioTieneAlcanceSobreRequisicionUseCase(usuarios, requisiciones);

        Assert.False(useCase.Ejecutar(usuario.Id, requisicionDeEmpresaB.Id));
    }

    [Fact]
    public void Requisicion_inexistente_permite_continuar_para_que_el_caso_de_uso_reporte_404()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);

        var useCase = new UsuarioTieneAlcanceSobreRequisicionUseCase(usuarios, new FakeRequisicionRepository());

        Assert.True(useCase.Ejecutar(usuario.Id, 999));
    }

    [Fact]
    public void Usuario_inexistente_esta_fuera_del_alcance()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var requisicion = CrearRequisicion(1, empresa);
        var requisiciones = new FakeRequisicionRepository();
        requisiciones.Guardar(requisicion);

        var useCase = new UsuarioTieneAlcanceSobreRequisicionUseCase(new FakeUsuarioRepository(), requisiciones);

        Assert.False(useCase.Ejecutar(999, requisicion.Id));
    }
}
