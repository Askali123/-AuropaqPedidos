using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakePermisoRepository : IPermisoRepository
{
    private readonly Dictionary<int, Permiso> _permisos = new();

    public void Agregar(Permiso permiso) => _permisos[permiso.Id] = permiso;

    public Permiso? ObtenerPorId(int id) => _permisos.TryGetValue(id, out var permiso) ? permiso : null;

    public IReadOnlyList<Permiso> ObtenerTodos() => _permisos.Values.OrderBy(p => p.Codigo).ToList();

    public bool ExisteParaCodigo(string codigo) => _permisos.Values.Any(p => p.Codigo == codigo);

    public void Guardar(Permiso permiso) => _permisos[permiso.Id] = permiso;
}
