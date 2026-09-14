using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IProductoRepository
{
    Producto? ObtenerPorId(int id);

    // Soporta el selector de producto del Frontend (TASK: habilitar consultas para Requisiciones).
    IReadOnlyList<Producto> ObtenerTodos();
}
