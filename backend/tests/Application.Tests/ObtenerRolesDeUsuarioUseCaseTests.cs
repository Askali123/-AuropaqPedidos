using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ObtenerRolesDeUsuarioUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    [Fact]
    public void Devuelve_los_roles_asignados_al_usuario()
    {
        var usuario = new Usuario(1, new Empresa(1, "AUROTECH"), "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var rolSolicitante = new Rol(1, "Solicitante");
        var rolRevisor = new Rol(2, "Revisor");

        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);
        var usuariosRoles = new FakeUsuarioRolRepository();
        usuariosRoles.Guardar(new UsuarioRol(usuario, rolSolicitante));
        usuariosRoles.Guardar(new UsuarioRol(usuario, rolRevisor));

        var respuesta = new ObtenerRolesDeUsuarioUseCase(usuarios, usuariosRoles).Ejecutar(usuario.Id);

        Assert.Equal(2, respuesta.Count);
        Assert.Contains(respuesta, r => r.Id == rolSolicitante.Id);
        Assert.Contains(respuesta, r => r.Id == rolRevisor.Id);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_el_usuario_no_tiene_roles_asignados()
    {
        var usuario = new Usuario(1, new Empresa(1, "AUROTECH"), "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);

        var respuesta = new ObtenerRolesDeUsuarioUseCase(usuarios, new FakeUsuarioRolRepository()).Ejecutar(usuario.Id);

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Lanza_no_encontrado_cuando_el_usuario_no_existe()
    {
        Assert.Throws<RecursoNoEncontradoException>(
            () => new ObtenerRolesDeUsuarioUseCase(new FakeUsuarioRepository(), new FakeUsuarioRolRepository()).Ejecutar(999));
    }
}
