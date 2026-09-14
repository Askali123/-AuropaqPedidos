using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class UsuarioTests
{
    private const string HashDePrueba = "hash-de-prueba";

    private static Empresa CrearEmpresa() => new(1, "AUROTECH");

    private static readonly DateTime Fecha = new(2026, 9, 14);

    [Fact]
    public void Crear_usuario_valido_queda_activo_y_asociado_a_su_empresa()
    {
        var empresa = CrearEmpresa();

        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", HashDePrueba, Fecha, apellido: "Gómez");

        Assert.Equal(empresa, usuario.Empresa);
        Assert.Equal("Martha", usuario.Nombre);
        Assert.Equal("Gómez", usuario.Apellido);
        Assert.Equal("martha@auropaq.com", usuario.Correo);
        Assert.Equal(HashDePrueba, usuario.PasswordHash);
        Assert.True(usuario.Activo);
        Assert.Equal(Fecha, usuario.FechaCreacion);
        Assert.Equal(Fecha, usuario.FechaActualizacion);
    }

    [Fact]
    public void Permite_crear_usuario_sin_apellido()
    {
        var usuario = new Usuario(1, CrearEmpresa(), "Martha", "martha@auropaq.com", HashDePrueba, Fecha);

        Assert.Null(usuario.Apellido);
    }

    [Fact]
    public void No_permite_crear_usuario_sin_empresa()
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Usuario(1, null!, "Martha", "martha@auropaq.com", HashDePrueba, Fecha));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_usuario_sin_nombre(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Usuario(1, CrearEmpresa(), nombreInvalido!, "martha@auropaq.com", HashDePrueba, Fecha));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_usuario_sin_correo(string? correoInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Usuario(1, CrearEmpresa(), "Martha", correoInvalido!, HashDePrueba, Fecha));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_usuario_sin_password_hash(string? hashInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Usuario(1, CrearEmpresa(), "Martha", "martha@auropaq.com", hashInvalido!, Fecha));
    }

    [Fact]
    public void Desactivar_y_activar_cambian_el_estado_y_la_fecha_de_actualizacion()
    {
        var usuario = new Usuario(1, CrearEmpresa(), "Martha", "martha@auropaq.com", HashDePrueba, Fecha);
        var fechaDesactivacion = Fecha.AddDays(1);
        var fechaActivacion = Fecha.AddDays(2);

        usuario.Desactivar(fechaDesactivacion);
        Assert.False(usuario.Activo);
        Assert.Equal(fechaDesactivacion, usuario.FechaActualizacion);

        usuario.Activar(fechaActivacion);
        Assert.True(usuario.Activo);
        Assert.Equal(fechaActivacion, usuario.FechaActualizacion);
    }
}
