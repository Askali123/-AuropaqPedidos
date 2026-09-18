using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// TASK-101 (docs/2026-09-18-auditoria-dominio-roles-frontend.md). Mismo escenario que
// UsuarioTienePermisoUseCaseTests, pero verificando la lista completa de permisos en vez de un
// booleano por código.
public class ObtenerMisPermisosUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 18);

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeUsuarioRolRepository UsuariosRoles { get; } = new();
        public FakeRolPermisoRepository RolesPermisos { get; } = new();
        public Usuario Usuario { get; }

        public Escenario()
        {
            var empresa = new Empresa(1, "AUROTECH");
            Usuario = new Usuario(1, empresa, "Carlos", "carlos@auropaq.com", "hash-de-prueba", Fecha);
            Usuarios.Guardar(Usuario);
        }

        public ObtenerMisPermisosUseCase UseCase() => new(Usuarios, UsuariosRoles, RolesPermisos);

        public void AsignarPermiso(Rol rol, Permiso permiso)
        {
            UsuariosRoles.Guardar(new UsuarioRol(Usuario, rol));
            RolesPermisos.Guardar(new RolPermiso(rol, permiso));
        }
    }

    [Fact]
    public void Devuelve_los_permisos_del_rol_asignado()
    {
        var escenario = new Escenario();
        var rol = new Rol(1, "Solicitante");
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        escenario.AsignarPermiso(rol, permiso);

        var respuesta = escenario.UseCase().Ejecutar(escenario.Usuario.Id);

        var unico = Assert.Single(respuesta);
        Assert.Equal("REQUISICION_CREAR", unico.Codigo);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_el_usuario_no_tiene_ningun_rol()
    {
        var escenario = new Escenario();

        Assert.Empty(escenario.UseCase().Ejecutar(escenario.Usuario.Id));
    }

    [Fact]
    public void Combina_los_permisos_de_multiples_roles_sin_duplicados()
    {
        var escenario = new Escenario();
        var rolA = new Rol(1, "Solicitante");
        var permisoX = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        var rolB = new Rol(2, "Gestor_Requisiciones");
        var permisoY = new Permiso(2, "REQUISICION_APROBAR", "Aprobar requisición");
        escenario.AsignarPermiso(rolA, permisoX);
        escenario.AsignarPermiso(rolB, permisoY);
        // Mismo permiso otorgado por un segundo rol: no debe duplicarse en la respuesta.
        escenario.UsuariosRoles.Guardar(new UsuarioRol(escenario.Usuario, rolB));
        escenario.RolesPermisos.Guardar(new RolPermiso(rolB, permisoX));

        var respuesta = escenario.UseCase().Ejecutar(escenario.Usuario.Id);

        Assert.Equal(2, respuesta.Count);
        Assert.Contains(respuesta, p => p.Codigo == "REQUISICION_CREAR");
        Assert.Contains(respuesta, p => p.Codigo == "REQUISICION_APROBAR");
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_el_usuario_esta_inactivo_aunque_tenga_permisos()
    {
        var escenario = new Escenario();
        var rol = new Rol(1, "Solicitante");
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        escenario.AsignarPermiso(rol, permiso);
        escenario.Usuario.Desactivar(Fecha);

        Assert.Empty(escenario.UseCase().Ejecutar(escenario.Usuario.Id));
    }

    [Fact]
    public void Lanza_no_encontrado_cuando_el_usuario_no_existe()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.UseCase().Ejecutar(999));
    }
}
