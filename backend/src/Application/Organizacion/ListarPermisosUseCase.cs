using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-011. Deja preparado el catálogo de Permisos para que TASK-013 pueda relacionarlos con
// Rol mediante RolPermiso. Devuelve todos los permisos sin filtrar (no hay campo Activo que
// filtrar, ni ninguna otra regla documentada para esta consulta).
public sealed class ListarPermisosUseCase
{
    private readonly IPermisoRepository _permisos;

    public ListarPermisosUseCase(IPermisoRepository permisos)
    {
        _permisos = permisos;
    }

    public IReadOnlyList<PermisoResponse> Ejecutar() =>
        _permisos.ObtenerTodos()
            .Select(p => new PermisoResponse(p.Id, p.Codigo, p.Nombre, p.Descripcion))
            .ToList();
}
