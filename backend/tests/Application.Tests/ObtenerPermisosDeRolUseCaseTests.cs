using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ObtenerPermisosDeRolUseCaseTests
{
    [Fact]
    public void Devuelve_los_permisos_asignados_al_rol()
    {
        var rol = new Rol(1, "Solicitante");
        var permisoCrear = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");
        var permisoVer = new Permiso(2, "REQUISICION_VER", "Ver requisición");

        var roles = new FakeRolRepository();
        roles.Agregar(rol);
        var rolesPermisos = new FakeRolPermisoRepository();
        rolesPermisos.Guardar(new RolPermiso(rol, permisoCrear));
        rolesPermisos.Guardar(new RolPermiso(rol, permisoVer));

        var respuesta = new ObtenerPermisosDeRolUseCase(roles, rolesPermisos).Ejecutar(rol.Id);

        Assert.Equal(2, respuesta.Count);
        Assert.Contains(respuesta, p => p.Id == permisoCrear.Id);
        Assert.Contains(respuesta, p => p.Id == permisoVer.Id);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_el_rol_no_tiene_permisos_asignados()
    {
        var rol = new Rol(1, "Solicitante");
        var roles = new FakeRolRepository();
        roles.Agregar(rol);

        var respuesta = new ObtenerPermisosDeRolUseCase(roles, new FakeRolPermisoRepository()).Ejecutar(rol.Id);

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Lanza_no_encontrado_cuando_el_rol_no_existe()
    {
        Assert.Throws<RecursoNoEncontradoException>(
            () => new ObtenerPermisosDeRolUseCase(new FakeRolRepository(), new FakeRolPermisoRepository()).Ejecutar(999));
    }
}
