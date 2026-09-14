using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// TASK-015. Correo inexistente, password incorrecta y usuario inactivo lanzan la misma excepción
// con el mismo mensaje (verificado explícitamente abajo) — TASK-015 §15: no revelar cuál de los
// tres ocurrió.
public class LoginUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);
    private const string PasswordValido = "password123";

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakePasswordHasher PasswordHasher { get; } = new();
        public FakeGeneradorDeToken GeneradorDeToken { get; } = new();
        public Usuario Usuario { get; }

        public Escenario()
        {
            var empresa = new Empresa(1, "AUROTECH");
            Usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", PasswordHasher.Hash(PasswordValido), Fecha);
            Usuarios.Guardar(Usuario);
        }

        public LoginUseCase UseCase() => new(Usuarios, PasswordHasher, GeneradorDeToken);
    }

    [Fact]
    public void Login_valido_devuelve_token_y_datos_del_usuario()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(new LoginRequest("martha@auropaq.com", PasswordValido), Fecha);

        Assert.False(string.IsNullOrWhiteSpace(respuesta.Token));
        Assert.Equal(Fecha.AddMinutes(60), respuesta.FechaExpiracion);
        Assert.Equal(escenario.Usuario.Id, respuesta.Usuario.Id);
        Assert.Equal(escenario.Usuario.Correo, respuesta.Usuario.Correo);
    }

    [Fact]
    public void Rechaza_correo_inexistente()
    {
        var escenario = new Escenario();

        var ex = Assert.Throws<CredencialesInvalidasException>(
            () => escenario.UseCase().Ejecutar(new LoginRequest("no-existe@auropaq.com", PasswordValido), Fecha));

        Assert.Equal("Correo o contraseña incorrectos.", ex.Message);
    }

    [Fact]
    public void Rechaza_password_incorrecta()
    {
        var escenario = new Escenario();

        var ex = Assert.Throws<CredencialesInvalidasException>(
            () => escenario.UseCase().Ejecutar(new LoginRequest("martha@auropaq.com", "otro-password"), Fecha));

        Assert.Equal("Correo o contraseña incorrectos.", ex.Message);
    }

    [Fact]
    public void Rechaza_usuario_inactivo_con_el_mismo_mensaje_generico()
    {
        var escenario = new Escenario();
        escenario.Usuario.Desactivar(Fecha);

        var ex = Assert.Throws<CredencialesInvalidasException>(
            () => escenario.UseCase().Ejecutar(new LoginRequest("martha@auropaq.com", PasswordValido), Fecha));

        Assert.Equal("Correo o contraseña incorrectos.", ex.Message);
    }
}
