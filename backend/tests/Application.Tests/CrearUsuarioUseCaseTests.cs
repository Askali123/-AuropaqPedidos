using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CrearUsuarioUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);
    private const string PasswordValido = "password123";

    private sealed class Escenario
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeEmpresaRepository Empresas { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public FakePasswordHasher PasswordHasher { get; } = new();
        public Empresa Empresa { get; } = new(1, "AUROTECH");

        public Escenario() => Empresas.Agregar(Empresa);

        public CrearUsuarioUseCase UseCase() => new(Usuarios, Empresas, Ids, PasswordHasher);
    }

    [Fact]
    public void Crea_un_usuario_asociado_a_su_empresa()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Martha", "martha@auropaq.com", PasswordValido, "Gómez"),
            Fecha);

        Assert.Equal(escenario.Empresa.Id, respuesta.EmpresaId);
        Assert.Equal("Martha", respuesta.Nombre);
        Assert.Equal("martha@auropaq.com", respuesta.Correo);
        Assert.True(respuesta.Activo);
        Assert.NotNull(escenario.Usuarios.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void No_permite_crear_usuario_con_empresa_inexistente()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(999, "Martha", "martha@auropaq.com", PasswordValido), Fecha));
    }

    [Fact]
    public void No_permite_dos_usuarios_con_el_mismo_correo()
    {
        var escenario = new Escenario();
        escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Martha", "martha@auropaq.com", PasswordValido), Fecha);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Otra Martha", "martha@auropaq.com", PasswordValido), Fecha));
    }

    [Fact]
    public void El_correo_duplicado_se_valida_de_forma_global_entre_empresas()
    {
        var escenario = new Escenario();
        var otraEmpresa = new Empresa(2, "COURIERBOX");
        escenario.Empresas.Agregar(otraEmpresa);
        escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Martha", "martha@auropaq.com", PasswordValido), Fecha);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(otraEmpresa.Id, "Otra Martha", "martha@auropaq.com", PasswordValido), Fecha));
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("")]
    public void No_permite_password_con_menos_de_8_caracteres(string passwordCorto)
    {
        var escenario = new Escenario();

        Assert.Throws<ReglaDeNegocioException>(() => escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Martha", "martha@auropaq.com", passwordCorto), Fecha));
    }

    [Fact]
    public void El_password_se_hashea_y_nunca_se_guarda_en_texto_plano()
    {
        var escenario = new Escenario();

        var respuesta = escenario.UseCase().Ejecutar(
            new CrearUsuarioRequest(escenario.Empresa.Id, "Martha", "martha@auropaq.com", PasswordValido), Fecha);

        var usuarioGuardado = escenario.Usuarios.ObtenerPorId(respuesta.Id)!;
        Assert.NotEqual(PasswordValido, usuarioGuardado.PasswordHash);
        Assert.Equal(escenario.PasswordHasher.Hash(PasswordValido), usuarioGuardado.PasswordHash);
    }
}
