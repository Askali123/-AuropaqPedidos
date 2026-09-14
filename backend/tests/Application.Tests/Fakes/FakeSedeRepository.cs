using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeSedeRepository : ISedeRepository
{
    private readonly Dictionary<int, Sede> _sedes = new();

    public void Agregar(Sede sede) => _sedes[sede.Id] = sede;

    public Sede? ObtenerPorId(int id) => _sedes.TryGetValue(id, out var sede) ? sede : null;

    public IReadOnlyList<Sede> ObtenerPorEmpresa(int empresaId) =>
        _sedes.Values.Where(s => s.Empresa.Id == empresaId).OrderBy(s => s.Nombre).ToList();
}
