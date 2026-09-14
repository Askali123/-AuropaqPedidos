using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class ListarPermisosUseCaseTests
{
    [Fact]
    public void Devuelve_todos_los_permisos()
    {
        var permisos = new FakePermisoRepository();
        permisos.Agregar(new Permiso(1, "REQUISICION_CREAR", "Crear requisición"));
        permisos.Agregar(new Permiso(2, "REQUISICION_VER", "Ver requisición"));

        var respuesta = new ListarPermisosUseCase(permisos).Ejecutar();

        Assert.Equal(2, respuesta.Count);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_permisos()
    {
        var respuesta = new ListarPermisosUseCase(new FakePermisoRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }
}
