using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-010. Devuelve todos los roles sin filtrar por Activo: mismo criterio ya usado en
// ListarEmpresasUseCase/ListarUsuariosUseCase (ninguna regla de negocio define ese filtro para
// esta consulta, no se inventa aquí).
public sealed class ListarRolesUseCase
{
    private readonly IRolRepository _roles;

    public ListarRolesUseCase(IRolRepository roles)
    {
        _roles = roles;
    }

    public IReadOnlyList<RolResponse> Ejecutar() =>
        _roles.ObtenerTodos()
            .Select(r => new RolResponse(r.Id, r.Nombre, r.Descripcion, r.Activo))
            .ToList();
}
