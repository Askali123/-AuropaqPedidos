using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

// Mismo criterio que UsuarioRolTests: Rol y Permiso son GLOBALES, no existe ninguna validación
// de empresa que probar aquí.
public class RolPermisoTests
{
    [Fact]
    public void Crear_asignacion_valida_asocia_rol_y_permiso()
    {
        var rol = new Rol(1, "Solicitante");
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");

        var rolPermiso = new RolPermiso(rol, permiso);

        Assert.Equal(rol, rolPermiso.Rol);
        Assert.Equal(permiso, rolPermiso.Permiso);
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_rol()
    {
        var permiso = new Permiso(1, "REQUISICION_CREAR", "Crear requisición");

        Assert.Throws<ReglaDeNegocioException>(() => new RolPermiso(null!, permiso));
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_permiso()
    {
        var rol = new Rol(1, "Solicitante");

        Assert.Throws<ReglaDeNegocioException>(() => new RolPermiso(rol, null!));
    }
}
