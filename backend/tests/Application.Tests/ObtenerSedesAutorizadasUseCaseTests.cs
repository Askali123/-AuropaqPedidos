using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ObtenerSedesAutorizadasUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 14);

    [Fact]
    public void Devuelve_las_sedes_asignadas_al_usuario()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var sedeBogota = new Sede(1, empresa, "Sede Bogotá");
        var sedeMedellin = new Sede(2, empresa, "Sede Medellín");

        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);
        var usuariosSedes = new FakeUsuarioSedeRepository();
        usuariosSedes.Guardar(new UsuarioSede(usuario, sedeBogota));
        usuariosSedes.Guardar(new UsuarioSede(usuario, sedeMedellin));

        var respuesta = new ObtenerSedesAutorizadasUseCase(usuarios, usuariosSedes).Ejecutar(usuario.Id);

        Assert.Equal(2, respuesta.Count);
        Assert.Contains(respuesta, s => s.Id == sedeBogota.Id);
        Assert.Contains(respuesta, s => s.Id == sedeMedellin.Id);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_el_usuario_no_tiene_sedes_asignadas()
    {
        var empresa = new Empresa(1, "AUROTECH");
        var usuario = new Usuario(1, empresa, "Martha", "martha@auropaq.com", "hash-de-prueba", Fecha);
        var usuarios = new FakeUsuarioRepository();
        usuarios.Guardar(usuario);

        var respuesta = new ObtenerSedesAutorizadasUseCase(usuarios, new FakeUsuarioSedeRepository()).Ejecutar(usuario.Id);

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Lanza_no_encontrado_cuando_el_usuario_no_existe()
    {
        Assert.Throws<RecursoNoEncontradoException>(
            () => new ObtenerSedesAutorizadasUseCase(new FakeUsuarioRepository(), new FakeUsuarioSedeRepository()).Ejecutar(999));
    }
}
