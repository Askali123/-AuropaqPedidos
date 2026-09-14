using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class AsignarPermisoARolUseCaseTests
{
    private sealed class Escenario
    {
        public FakeRolRepository Roles { get; } = new();
        public FakePermisoRepository Permisos { get; } = new();
        public FakeRolPermisoRepository RolesPermisos { get; } = new();
        public Rol Rol { get; }
        public Permiso Permiso { get; }

        public Escenario()
        {
            Rol = new Rol(1, "Solicitante");
            Permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
            Roles.Agregar(Rol);
            Permisos.Agregar(Permiso);
        }

        public AsignarPermisoARolUseCase UseCase() => new(RolesPermisos, Roles, Permisos);
    }

    [Fact]
    public void Asigna_un_permiso_a_un_rol()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(escenario.Rol.Id, escenario.Permiso.Id);

        Assert.Equal(escenario.Rol.Id, respuesta.RolId);
        Assert.Equal(escenario.Permiso.Id, respuesta.PermisoId);
        Assert.True(escenario.RolesPermisos.Existe(escenario.Rol.Id, escenario.Permiso.Id));
    }

    [Fact]
    public void No_permite_asignar_permiso_a_un_rol_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(999, escenario.Permiso.Id));
    }

    [Fact]
    public void No_permite_asignar_un_permiso_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(
            () => escenario.UseCase().Ejecutar(escenario.Rol.Id, 999));
    }

    [Fact]
    public void No_permite_asignar_el_mismo_permiso_dos_veces()
    {
        var escenario = new Escenario();
        escenario.UseCase().Ejecutar(escenario.Rol.Id, escenario.Permiso.Id);

        Assert.Throws<ReglaDeNegocioException>(
            () => escenario.UseCase().Ejecutar(escenario.Rol.Id, escenario.Permiso.Id));
    }

    [Fact]
    public void Un_rol_puede_tener_varios_permisos()
    {
        var escenario = new Escenario();
        var segundoPermiso = new Permiso(2, "REQUISICION_VER", "Ver requisición");
        escenario.Permisos.Agregar(segundoPermiso);

        escenario.UseCase().Ejecutar(escenario.Rol.Id, escenario.Permiso.Id);
        escenario.UseCase().Ejecutar(escenario.Rol.Id, segundoPermiso.Id);

        Assert.Equal(2, escenario.RolesPermisos.ObtenerPorRol(escenario.Rol.Id).Count);
    }
}
