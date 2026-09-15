using AuropaqPedidos.Application.Auditorias.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeAuditoriaRepository : IAuditoriaRepository
{
    private readonly List<Auditoria> _registros = new();
    public IReadOnlyList<Auditoria> Registros => _registros;

    public void Guardar(Auditoria auditoria) => _registros.Add(auditoria);
}
