using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

// A diferencia de UsuarioSedeTests, no existe una prueba de "usuario y rol de distinta empresa":
// Rol es GLOBAL (04-base-datos.md §9.1 no declara EmpresaId), así que esa validación no aplica
// aquí — no se inventa una regla que la documentación no exige.
public class UsuarioRolTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private static Usuario CrearUsuario() =>
        new(1, new Empresa(1, "AUROTECH"), "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);

    [Fact]
    public void Crear_asignacion_valida_asocia_usuario_y_rol()
    {
        var usuario = CrearUsuario();
        var rol = new Rol(1, "Solicitante");

        var usuarioRol = new UsuarioRol(usuario, rol);

        Assert.Equal(usuario, usuarioRol.Usuario);
        Assert.Equal(rol, usuarioRol.Rol);
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_usuario()
    {
        var rol = new Rol(1, "Solicitante");

        Assert.Throws<ReglaDeNegocioException>(() => new UsuarioRol(null!, rol));
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_rol()
    {
        var usuario = CrearUsuario();

        Assert.Throws<ReglaDeNegocioException>(() => new UsuarioRol(usuario, null!));
    }

    [Fact]
    public void Un_usuario_de_una_empresa_puede_tener_un_rol_global_sin_restriccion_de_empresa()
    {
        // Confirma explícitamente que Rol (global) no exige ninguna relación con la Empresa del
        // Usuario — a diferencia de Sede, que sí la exige (UsuarioSedeTests).
        var usuario = CrearUsuario();
        var rol = new Rol(1, "Administrador");

        var usuarioRol = new UsuarioRol(usuario, rol);

        Assert.NotNull(usuarioRol);
    }
}
