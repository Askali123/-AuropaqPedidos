using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class AsignarUsuarioASedeUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeSedeRepository Sedes { get; } = new();
        public FakeUsuarioSedeRepository UsuariosSedes { get; } = new();
        public Empresa EmpresaA { get; } = new(1, "AUROTECH");
        public Empresa EmpresaB { get; } = new(2, "COURIERBOX");
        public Usuario Usuario { get; }
        public Sede Sede { get; }

        public Escenario()
        {
            Usuario = new Usuario(1, EmpresaA, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
            Sede = new Sede(1, EmpresaA, "Sede Bogotá");
            Usuarios.Guardar(Usuario);
            Sedes.Agregar(Sede);
        }

        public AsignarUsuarioASedeUseCase UseCase() => new(UsuariosSedes, Usuarios, Sedes);
    }

    [Fact]
    public void Asigna_un_usuario_a_una_sede_de_su_misma_empresa()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Sede.Id);

        Assert.Equal(escenario.Usuario.Id, respuesta.UsuarioId);
        Assert.Equal(escenario.Sede.Id, respuesta.SedeId);
        Assert.True(escenario.UsuariosSedes.Existe(escenario.Usuario.Id, escenario.Sede.Id));
    }

    [Fact]
    public void No_permite_asignar_un_usuario_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(999, escenario.Sede.Id));
    }

    [Fact]
    public void No_permite_asignar_una_sede_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, 999));
    }

    [Fact]
    public void No_permite_asignar_un_usuario_a_una_sede_de_otra_empresa()
    {
        var escenario = new Escenario();
        var sedeDeOtraEmpresa = new Sede(2, escenario.EmpresaB, "Sede Medellín");
        escenario.Sedes.Agregar(sedeDeOtraEmpresa);

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, sedeDeOtraEmpresa.Id));
    }

    [Fact]
    public void No_permite_asignar_la_misma_relacion_dos_veces()
    {
        var escenario = new Escenario();
        escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Sede.Id);

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Sede.Id));
    }
}
