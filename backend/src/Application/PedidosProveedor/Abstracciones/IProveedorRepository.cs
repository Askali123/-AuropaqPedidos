using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

public interface IProveedorRepository
{
    Proveedor? ObtenerPorId(int id);
}
