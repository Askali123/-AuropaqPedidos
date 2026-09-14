using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// TASK-016. Fuente: Usuario -> UsuarioRol -> Rol -> RolPermiso -> Permiso.Codigo.
public class UsuarioTienePermisoUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeUsuarioRolRepository UsuariosRoles { get; } = new();
        public FakeRolPermisoRepository RolesPermisos { get; } = new();
        public Usuario Usuario { get; }

        public Escenario()
        {
            var empresa = new Empresa(1, "AUROTECH");
            Usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
            Usuarios.Guardar(Usuario);
        }

        public UsuarioTienePermisoUseCase UseCase() => new(Usuarios, UsuariosRoles, RolesPermisos);

        public void AsignarPermiso(Rol rol, Permiso permiso)
        {
            UsuariosRoles.Guardar(new UsuarioRol(Usuario, rol));
            RolesPermisos.Guardar(new RolPermiso(rol, permiso));
        }
    }

    [Fact]
    public void Usuario_con_permiso_via_su_rol_esta_autorizado()
    {
        var escenario = new Escenario();
        var rol = new Rol(1, "Gestor");
        var permiso = new Permiso(1, "REQUISICION_APROBAR", "Aprobar requisición");
        escenario.AsignarPermiso(rol, permiso);

        Assert.True(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_APROBAR"));
    }

    [Fact]
    public void Usuario_sin_ese_permiso_no_esta_autorizado()
    {
        var escenario = new Escenario();
        var rol = new Rol(1, "Solicitante");
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        escenario.AsignarPermiso(rol, permiso);

        Assert.False(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_APROBAR"));
    }

    [Fact]
    public void Usuario_sin_ningun_rol_no_esta_autorizado()
    {
        var escenario = new Escenario();

        Assert.False(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_APROBAR"));
    }

    [Fact]
    public void Usuario_inexistente_no_esta_autorizado()
    {
        var escenario = new Escenario();

        Assert.False(escenario.UseCase().Ejecutar(999, "REQUISICION_APROBAR"));
    }

    [Fact]
    public void Usuario_inactivo_no_esta_autorizado_aunque_tenga_el_permiso()
    {
        var escenario = new Escenario();
        var rol = new Rol(1, "Gestor");
        var permiso = new Permiso(1, "REQUISICION_APROBAR", "Aprobar requisición");
        escenario.AsignarPermiso(rol, permiso);
        escenario.Usuario.Desactivar(Fecha);

        Assert.False(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_APROBAR"));
    }

    [Fact]
    public void Usuario_con_multiples_roles_puede_usar_el_permiso_de_cualquiera_de_ellos()
    {
        var escenario = new Escenario();
        var rolA = new Rol(1, "Solicitante");
        var permisoX = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        var rolB = new Rol(2, "Gestor");
        var permisoY = new Permiso(2, "REQUISICION_APROBAR", "Aprobar requisición");
        escenario.AsignarPermiso(rolA, permisoX);
        escenario.AsignarPermiso(rolB, permisoY);

        Assert.True(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_CREAR"));
        Assert.True(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_APROBAR"));
        Assert.False(escenario.UseCase().Ejecutar(escenario.Usuario.Id, "REQUISICION_DEVOLVER"));
    }
}
