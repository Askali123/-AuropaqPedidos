using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeUnidadMedidaRepository : IUnidadMedidaRepository
{
    private readonly Dictionary<int, UnidadMedida> _unidadesMedida = new();

    public void Agregar(UnidadMedida unidadMedida) => _unidadesMedida[unidadMedida.Id] = unidadMedida;

    public UnidadMedida? ObtenerPorId(int id) => _unidadesMedida.TryGetValue(id, out var unidadMedida) ? unidadMedida : null;

    public IReadOnlyList<UnidadMedida> ObtenerTodas() => _unidadesMedida.Values.OrderBy(u => u.Nombre).ToList();

    public void Guardar(UnidadMedida unidadMedida) => _unidadesMedida[unidadMedida.Id] = unidadMedida;
}
