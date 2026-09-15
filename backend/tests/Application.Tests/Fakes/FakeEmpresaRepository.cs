using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeEmpresaRepository : IEmpresaRepository
{
    private readonly Dictionary<int, Empresa> _empresas = new();

    public void Agregar(Empresa empresa) => _empresas[empresa.Id] = empresa;

    public Empresa? ObtenerPorId(int id) => _empresas.TryGetValue(id, out var empresa) ? empresa : null;

    public IReadOnlyList<Empresa> ObtenerTodas() => _empresas.Values.OrderBy(e => e.Nombre).ToList();

    public void Guardar(Empresa empresa) => _empresas[empresa.Id] = empresa;
}
