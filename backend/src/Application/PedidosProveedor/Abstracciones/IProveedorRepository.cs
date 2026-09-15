using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

public interface IProveedorRepository
{
    Proveedor? ObtenerPorId(int id);

    // TASK-018, 05-api.md §29.
    IReadOnlyList<Proveedor> ObtenerTodos();

    void Guardar(Proveedor proveedor);
}
