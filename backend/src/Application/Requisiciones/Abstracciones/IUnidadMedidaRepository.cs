using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// TASK-015, 05-api.md §15.
public interface IUnidadMedidaRepository
{
    UnidadMedida? ObtenerPorId(int id);

    IReadOnlyList<UnidadMedida> ObtenerTodas();

    void Guardar(UnidadMedida unidadMedida);
}
