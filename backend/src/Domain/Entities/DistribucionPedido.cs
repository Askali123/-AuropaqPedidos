using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-032, 02-dominio.md §23, 04-base-datos.md §28: hacia qué sede se destina una cantidad
// comprada. A diferencia de DistribucionRequisicion, la Sede puede pertenecer a cualquier
// empresa (un PedidoProveedor no pertenece a una única empresa — 02-dominio.md §21). No existe
// una regla documentada equivalente a RN-011 ("SUM(distribuciones) = cantidad") para pedidos
// (ver auditoría 2026-09-10, Pendiente 6/7): no se implementa esa validación aquí para no
// inventarla. Solo se crea/muta a través de DetallePedidoProveedor (constructor internal).
public sealed class DistribucionPedido
{
    public int Id { get; }
    public Sede Sede { get; }
    public int Cantidad { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DistribucionPedido()
    {
    }
#pragma warning restore CS8618

    internal DistribucionPedido(int id, Sede sede, int cantidad)
    {
        if (sede is null)
            throw new ReglaDeNegocioException("Una distribución de pedido debe tener una sede.");

        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad distribuida debe ser mayor que cero.");

        Id = id;
        Sede = sede;
        Cantidad = cantidad;
    }
}
