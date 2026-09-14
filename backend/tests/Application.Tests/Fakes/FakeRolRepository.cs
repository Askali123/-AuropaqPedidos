using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeRolRepository : IRolRepository
{
    private readonly Dictionary<int, Rol> _roles = new();

    public void Agregar(Rol rol) => _roles[rol.Id] = rol;

    public Rol? ObtenerPorId(int id) => _roles.TryGetValue(id, out var rol) ? rol : null;

    public IReadOnlyList<Rol> ObtenerTodos() => _roles.Values.OrderBy(r => r.Nombre).ToList();

    public void Guardar(Rol rol) => _roles[rol.Id] = rol;
}
