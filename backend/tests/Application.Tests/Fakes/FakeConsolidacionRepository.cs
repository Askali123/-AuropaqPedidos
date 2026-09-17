using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeConsolidacionRepository : IConsolidacionRepository
{
    private readonly Dictionary<int, Consolidacion> _consolidaciones = new();

    public IReadOnlyCollection<Consolidacion> Todas => _consolidaciones.Values;

    public Consolidacion? ObtenerPorId(int id) => _consolidaciones.TryGetValue(id, out var consolidacion) ? consolidacion : null;

    public IReadOnlyList<Consolidacion> Listar(int? periodoId) =>
        _consolidaciones.Values
            .Where(c => periodoId is null || c.Periodo.Id == periodoId)
            .OrderByDescending(c => c.FechaCreacion)
            .ToList();

    public IReadOnlyList<int> ObtenerIdsDetallesRequisicionYaConsolidados() =>
        _consolidaciones.Values
            .SelectMany(c => c.Detalles)
            .SelectMany(d => d.Asignaciones)
            .Select(a => a.DetalleRequisicionOrigen.Id)
            .Distinct()
            .ToList();

    public void Guardar(Consolidacion consolidacion) => _consolidaciones[consolidacion.Id] = consolidacion;
}
