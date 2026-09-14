using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;

namespace Application.Tests;

public class CrearRolUseCaseTests
{
    [Fact]
    public void Crea_un_rol_activo()
    {
        var roles = new FakeRolRepository();

        var respuesta = new CrearRolUseCase(roles, new FakeGeneradorDeIdentificadores())
            .Ejecutar(new CrearRolRequest("Solicitante", "Puede crear requisiciones"));

        Assert.Equal("Solicitante", respuesta.Nombre);
        Assert.Equal("Puede crear requisiciones", respuesta.Descripcion);
        Assert.True(respuesta.Activo);
        Assert.NotNull(roles.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Permite_crear_dos_roles_con_el_mismo_nombre()
    {
        // 04-base-datos.md §9.1 no documenta unicidad de Nombre para Rol (a diferencia de
        // Usuario.Correo) — se confirma explícitamente que no se inventó esa restricción.
        var roles = new FakeRolRepository();
        var useCase = new CrearRolUseCase(roles, new FakeGeneradorDeIdentificadores());
        useCase.Ejecutar(new CrearRolRequest("Solicitante"));

        var segundo = useCase.Ejecutar(new CrearRolRequest("Solicitante"));

        Assert.Equal(2, roles.ObtenerTodos().Count);
        Assert.Equal("Solicitante", segundo.Nombre);
    }
}
