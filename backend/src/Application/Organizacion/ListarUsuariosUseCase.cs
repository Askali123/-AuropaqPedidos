using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-008. Devuelve todos los usuarios sin filtrar por Activo: mismo criterio ya usado en
// ListarEmpresasUseCase/ListarPeriodosUseCase (ninguna regla de negocio define ese filtro para
// esta consulta, no se inventa aquí).
public sealed class ListarUsuariosUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public ListarUsuariosUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public IReadOnlyList<UsuarioResponse> Ejecutar() =>
        _usuarios.ObtenerTodos()
            .Select(u => new UsuarioResponse(
                u.Id, u.Empresa.Id, u.Nombre, u.Apellido, u.Correo, u.Activo, u.FechaCreacion, u.FechaActualizacion))
            .ToList();
}
