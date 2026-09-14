using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// Cobertura agregada como parte del hardening previo a TASK-013 (reportado como deuda de tests
// al cerrar TASK-011: ListarUsuariosUseCase no tenía test dedicado, a diferencia de la mayoría
// de los demás ListarXUseCase). No modifica el comportamiento del use case.
public class ListarUsuariosUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    [Fact]
    public void Devuelve_todos_los_usuarios_con_su_empresa()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha, "Gómez"));
        usuarios.Guardar(new Usuario(2, empresa, "Juan", "juan@auropaq.com", "hash-de-prueba", Fecha));

        var respuesta = new ListarUsuariosUseCase(usuarios).Ejecutar();

        Assert.Equal(2, respuesta.Count);
        var martha = Assert.Single(respuesta, u => u.Id == 1);
        Assert.Equal(empresa.Id, martha.EmpresaId);
        Assert.Equal("Gómez", martha.Apellido);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_usuarios()
    {
        var respuesta = new ListarUsuariosUseCase(new FakeUsuarioRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Incluye_usuarios_inactivos_sin_filtrarlos()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        usuario.Desactivar(Fecha);
        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);

        var respuesta = new ListarUsuariosUseCase(usuarios).Ejecutar();

        var dto = Assert.Single(respuesta);
        Assert.False(dto.Activo);
    }
}
