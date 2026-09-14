using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class UsuarioSedeTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    private static Usuario CrearUsuario(Empresa empresa) =>
        new(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);

    [Fact]
    public void Crear_asignacion_valida_asocia_usuario_y_sede()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = CrearUsuario(empresa);
        var sede = new Sede(1, empresa, "Sede Bogotá");

        var usuarioSede = new UsuarioSede(usuario, sede);

        Assert.Equal(usuario, usuarioSede.Usuario);
        Assert.Equal(sede, usuarioSede.Sede);
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_usuario()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var sede = new Sede(1, empresa, "Sede Bogotá");

        Assert.Throws<ReglaDeNegocioException>(() => new UsuarioSede(null!, sede));
    }

    [Fact]
    public void No_permite_crear_asignacion_sin_sede()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = CrearUsuario(empresa);

        Assert.Throws<ReglaDeNegocioException>(() => new UsuarioSede(usuario, null!));
    }

    [Fact]
    public void No_permite_asignar_un_usuario_a_una_sede_de_otra_empresa()
    {
        var empresaA = new Empresa(1, "AUROTECH");
        var empresaB = new Empresa(2, "COURIERBOX");
        var usuario = CrearUsuario(empresaA);
        var sedeDeOtraEmpresa = new Sede(1, empresaB, "Sede Medellín");

        Assert.Throws<ReglaDeNegocioException>(() => new UsuarioSede(usuario, sedeDeOtraEmpresa));
    }
}
