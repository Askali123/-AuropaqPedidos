using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

// No existe aquí una prueba de "usuario y rol de distinta empresa" (a diferencia de
// AsignarUsuarioASedeUseCaseTests): Rol es GLOBAL, esa validación no aplica — confirmado
// explícitamente en UsuarioRolTests.Un_usuario_de_una_empresa_puede_tener_un_rol_global...
public class AsignarRolAUsuarioUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeRolRepository Roles { get; } = new();
        public FakeUsuarioRolRepository UsuariosRoles { get; } = new();
        public Usuario Usuario { get; }
        public Rol Rol { get; }

        public Escenario()
        {
            Usuario = new Usuario(1, new Empresa(1, "AUROTECH"), "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
            Rol = new Rol(1, "Solicitante");
            Usuarios.Guardar(Usuario);
            Roles.Agregar(Rol);
        }

        public AsignarRolAUsuarioUseCase UseCase() => new(UsuariosRoles, Usuarios, Roles);
    }

    [Fact]
    public void Asigna_un_rol_a_un_usuario()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Rol.Id);

        Assert.Equal(escenario.Usuario.Id, respuesta.UsuarioId);
        Assert.Equal(escenario.Rol.Id, respuesta.RolId);
        Assert.True(escenario.UsuariosRoles.Existe(escenario.Usuario.Id, escenario.Rol.Id));
    }

    [Fact]
    public void No_permite_asignar_rol_a_un_usuario_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(999, escenario.Rol.Id));
    }

    [Fact]
    public void No_permite_asignar_un_rol_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, 999));
    }

    [Fact]
    public void No_permite_asignar_el_mismo_rol_dos_veces()
    {
        var escenario = new Escenario();
        escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Rol.Id);

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Rol.Id));
    }

    [Fact]
    public void Un_usuario_puede_tener_varios_roles()
    {
        var escenario = new Escenario();
        var segundoRol = new Rol(2, "Revisor");
        escenario.Roles.Agregar(segundoRol);

        escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Rol.Id);
        escenario.UseCase().Ejecutar(escenario.Usuario.Id, segundoRol.Id);

        Assert.Equal(2, escenario.UsuariosRoles.ObtenerPorUsuario(escenario.Usuario.Id).Count);
    }
}
