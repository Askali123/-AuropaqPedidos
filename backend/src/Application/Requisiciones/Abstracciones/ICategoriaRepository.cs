using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// TASK-014, 05-api.md §14.
public interface ICategoriaRepository
{
    Categoria? ObtenerPorId(int id);

    IReadOnlyList<Categoria> ObtenerTodas();

    void Guardar(Categoria categoria);
}
