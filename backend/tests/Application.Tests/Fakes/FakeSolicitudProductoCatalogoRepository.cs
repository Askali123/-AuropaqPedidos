using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;

namespace Application.Tests.Fakes;

internal sealed class FakeSolicitudProductoCatalogoRepository : ISolicitudProductoCatalogoRepository
{
    private readonly Dictionary<int, SolicitudProductoCatalogo> _solicitudes = new();

    public void Agregar(SolicitudProductoCatalogo solicitud) => _solicitudes[solicitud.Id] = solicitud;

    public SolicitudProductoCatalogo? ObtenerPorId(int id) => _solicitudes.TryGetValue(id, out var solicitud) ? solicitud : null;

    public IReadOnlyList<SolicitudProductoCatalogo> ObtenerPendientes() =>
        _solicitudes.Values
            .Where(s => s.Estado == SolicitudProductoCatalogoEstado.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToList();

    public void Guardar(SolicitudProductoCatalogo solicitud) => _solicitudes[solicitud.Id] = solicitud;
}
