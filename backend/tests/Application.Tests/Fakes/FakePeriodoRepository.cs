using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakePeriodoRepository : IPeriodoRepository
{
    private readonly Dictionary<int, Periodo> _periodos = new();

    public void Agregar(Periodo periodo) => _periodos[periodo.Id] = periodo;

    public Periodo? ObtenerPorId(int id) => _periodos.TryGetValue(id, out var periodo) ? periodo : null;

    public IReadOnlyList<Periodo> ObtenerTodos() =>
        _periodos.Values.OrderBy(p => p.Anio).ThenBy(p => p.Mes).ToList();

    public bool ExisteParaAnioYMes(int anio, int mes) =>
        _periodos.Values.Any(p => p.Anio == anio && p.Mes == mes);

    public void Guardar(Periodo periodo) => _periodos[periodo.Id] = periodo;
}
