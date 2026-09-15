using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

// No existe aquí una prueba de "usuario y rol de distinta empresa" (a diferencia de
// AsignarUsuarioASedeUseCaseTests): Rol es GLOBAL, esa validación no aplica — confirmado
// explícitamente en UsuarioRolTests.Un_usuario_de_una_empresa_puede_tener_un_rol_global...
public class AsignarRolAUsuarioUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private const int UsuarioActorId = 999999;

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeRolRepository Roles { get; } = new();
        public FakeUsuarioRolRepository UsuariosRoles { get; } = new();
        public FakeAuditoriaRepository Auditoria { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public Usuario Usuario { get; }
        public Rol Rol { get; }

        public Escenario()
        {
            Usuario = new Usuario(1, new Empresa(1, "AUROTECH"), "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
            Rol = new Rol(1, "Solicitante");
            Usuarios.Guardar(Usuario);
            Roles.Agregar(Rol);
        }

        public AsignarRolAUsuarioUseCase UseCase() =>
            new(UsuariosRoles, Usuarios, Roles, Auditoria, Ids, NullLogger<AsignarRolAUsuarioUseCase>.Instance);
    }

    [Fact]
    public void Asigna_un_rol_a_un_usuario()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, escenario.Rol.Id, Fecha);

        Assert.Equal(escenario.Usuario.Id, respuesta.UsuarioId);
        Assert.Equal(escenario.Rol.Id, respuesta.RolId);
        Assert.True(escenario.UsuariosRoles.Existe(escenario.Usuario.Id, escenario.Rol.Id));
    }

    // TASK-056: confirma que asignar un rol queda registrado como "cambio administrativo".
    [Fact]
    public void Asignar_un_rol_registra_un_evento_de_auditoria()
    {
        var escenario = new Escenario();

        escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, escenario.Rol.Id, Fecha);

        var registro = Assert.Single(escenario.Auditoria.Registros);
        Assert.Equal("Usuario", registro.Entidad);
        Assert.Equal(escenario.Usuario.Id, registro.EntidadId);
        Assert.Equal("ASIGNAR_ROL", registro.Accion);
        Assert.Equal(UsuarioActorId, registro.UsuarioId);
    }

    [Fact]
    public void No_permite_asignar_rol_a_un_usuario_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(UsuarioActorId, 999, escenario.Rol.Id, Fecha));
    }

    [Fact]
    public void No_permite_asignar_un_rol_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, 999, Fecha));
    }

    [Fact]
    public void No_permite_asignar_el_mismo_rol_dos_veces()
    {
        var escenario = new Escenario();
        escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, escenario.Rol.Id, Fecha);

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, escenario.Rol.Id, Fecha));
    }

    [Fact]
    public void Un_usuario_puede_tener_varios_roles()
    {
        var escenario = new Escenario();
        var segundoRol = new Rol(2, "Revisor");
        escenario.Roles.Agregar(segundoRol);

        escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, escenario.Rol.Id, Fecha);
        escenario.UseCase().Ejecutar(UsuarioActorId, escenario.Usuario.Id, segundoRol.Id, Fecha);

        Assert.Equal(2, escenario.UsuariosRoles.ObtenerPorUsuario(escenario.Usuario.Id).Count);
    }

    // RN-060 punto 7 (06-seguridad.md §62): prevención de escalamiento de privilegios.
    [Fact]
    public void No_permite_que_un_usuario_se_asigne_un_rol_a_si_mismo()
    {
        var escenario = new Escenario();

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(escenario.Usuario.Id, escenario.Usuario.Id, escenario.Rol.Id, Fecha));
    }
}
