using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017, 05-api.md §28.2.
public sealed class ListarSolicitudesPendientesUseCase
{
    private readonly ISolicitudProductoCatalogoRepository _solicitudes;

    public ListarSolicitudesPendientesUseCase(ISolicitudProductoCatalogoRepository solicitudes)
    {
        _solicitudes = solicitudes;
    }

    public IReadOnlyList<SolicitudProductoCatalogoResponse> Ejecutar() =>
        _solicitudes.ObtenerPendientes()
            .Select(SolicitudProductoCatalogoMapper.AResponse)
            .ToList();
}
