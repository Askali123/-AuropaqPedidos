using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CrearPermisoUseCaseTests
{
    [Fact]
    public void Crea_un_permiso()
    {
        var permisos = new FakePermisoRepository();

        var respuesta = new CrearPermisoUseCase(permisos, new FakeGeneradorDeIdentificadores())
            .Ejecutar(new CrearPermisoRequest("REQUISICION_CREAR", "Crear requisición", "Permite crear una requisición"));

        Assert.Equal("REQUISICION_CREAR", respuesta.Codigo);
        Assert.Equal("Crear requisición", respuesta.Nombre);
        Assert.NotNull(permisos.ObtenerPorId(respuesta.Id));
    }

    // RN-055/ADR-056 (hardening previo a TASK-013): reemplaza a
    // "Permite_crear_dos_permisos_con_el_mismo_codigo" — el comportamiento documentado cambió
    // explícitamente de "sin unicidad" a "único GLOBAL" en esta tarea.
    [Fact]
    public void No_permite_crear_dos_permisos_con_el_mismo_codigo()
    {
        var permisos = new FakePermisoRepository();
        var useCase = new CrearPermisoUseCase(permisos, new FakeGeneradorDeIdentificadores());
        useCase.Ejecutar(new CrearPermisoRequest("REQUISICION_CREAR", "Crear requisición"));

        Assert.Throws<ReglaDeNegocioException>(
            () => useCase.Ejecutar(new CrearPermisoRequest("REQUISICION_CREAR", "Crear requisición (duplicado)")));

        Assert.Single(permisos.ObtenerTodos());
    }
}
