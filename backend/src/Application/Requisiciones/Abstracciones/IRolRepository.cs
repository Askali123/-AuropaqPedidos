using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IRolRepository
{
    Rol? ObtenerPorId(int id);

    IReadOnlyList<Rol> ObtenerTodos();

    void Guardar(Rol rol);
}
