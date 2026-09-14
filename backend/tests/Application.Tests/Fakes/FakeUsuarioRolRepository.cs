using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeUsuarioRolRepository : IUsuarioRolRepository
{
    private readonly List<UsuarioRol> _asignaciones = new();

    public bool Existe(int usuarioId, int rolId) =>
        _asignaciones.Any(ur => ur.Usuario.Id == usuarioId && ur.Rol.Id == rolId);

    public IReadOnlyList<UsuarioRol> ObtenerPorUsuario(int usuarioId) =>
        _asignaciones.Where(ur => ur.Usuario.Id == usuarioId).ToList();

    public void Guardar(UsuarioRol usuarioRol)
    {
        if (!_asignaciones.Contains(usuarioRol))
            _asignaciones.Add(usuarioRol);
    }
}
