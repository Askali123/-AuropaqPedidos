using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// TASK-017, 04-base-datos.md §13, 05-api.md §28.
public interface ISolicitudProductoCatalogoRepository
{
    SolicitudProductoCatalogo? ObtenerPorId(int id);

    // 05-api.md §28.2.
    IReadOnlyList<SolicitudProductoCatalogo> ObtenerPendientes();

    void Guardar(SolicitudProductoCatalogo solicitud);
}
