using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeUsuarioSedeRepository : IUsuarioSedeRepository
{
    private readonly List<UsuarioSede> _asignaciones = new();

    public bool Existe(int usuarioId, int sedeId) =>
        _asignaciones.Any(us => us.Usuario.Id == usuarioId && us.Sede.Id == sedeId);

    public IReadOnlyList<UsuarioSede> ObtenerPorUsuario(int usuarioId) =>
        _asignaciones.Where(us => us.Usuario.Id == usuarioId).ToList();

    public void Guardar(UsuarioSede usuarioSede)
    {
        if (!_asignaciones.Contains(usuarioSede))
            _asignaciones.Add(usuarioSede);
    }
}
