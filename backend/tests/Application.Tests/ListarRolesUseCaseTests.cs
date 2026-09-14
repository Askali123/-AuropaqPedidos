using Application.Tests.Fakes;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

// Cobertura agregada como parte del hardening previo a TASK-013 (reportado como deuda de tests
// al cerrar TASK-011: ListarRolesUseCase no tenía test dedicado, a diferencia de la mayoría de
// los demás ListarXUseCase). No modifica el comportamiento del use case.
public class ListarRolesUseCaseTests
{
    [Fact]
    public void Devuelve_todos_los_roles()
    {
        var roles = new FakeRolRepository();
        roles.Agregar(new Rol(1, "Solicitante", "Puede crear requisiciones"));
        roles.Agregar(new Rol(2, "Revisor"));

        var respuesta = new ListarRolesUseCase(roles).Ejecutar();

        Assert.Equal(2, respuesta.Count);
        var solicitante = Assert.Single(respuesta, r => r.Id == 1);
        Assert.Equal("Puede crear requisiciones", solicitante.Descripcion);
    }

    [Fact]
    public void Devuelve_lista_vacia_cuando_no_hay_roles()
    {
        var respuesta = new ListarRolesUseCase(new FakeRolRepository()).Ejecutar();

        Assert.Empty(respuesta);
    }

    [Fact]
    public void Incluye_roles_inactivos_sin_filtrarlos()
    {
        var rol = new Rol(1, "Solicitante");
        rol.Desactivar();
        var roles = new FakeRolRepository();
        roles.Agregar(rol);

        var respuesta = new ListarRolesUseCase(roles).Ejecutar();

        var dto = Assert.Single(respuesta);
        Assert.False(dto.Activo);
    }
}
