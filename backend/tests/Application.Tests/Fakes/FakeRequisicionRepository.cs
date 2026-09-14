using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;

namespace Application.Tests.Fakes;

internal sealed class FakeRequisicionRepository : IRequisicionRepository
{
    private readonly Dictionary<int, Requisicion> _requisiciones = new();

    public Requisicion? ObtenerPorId(int id) =>
        _requisiciones.TryGetValue(id, out var requisicion) ? requisicion : null;

    public Requisicion? ObtenerPorEmpresaYPeriodo(int empresaId, int periodoId) =>
        _requisiciones.Values.FirstOrDefault(r => r.Empresa.Id == empresaId && r.Periodo.Id == periodoId);

    public IReadOnlyList<Requisicion> ObtenerAprobadasPorPeriodo(int periodoId) =>
        _requisiciones.Values
            .Where(r => r.Periodo.Id == periodoId && r.Estado == RequisicionEstado.Aprobada)
            .ToList();

    public void Guardar(Requisicion requisicion) => _requisiciones[requisicion.Id] = requisicion;
}
