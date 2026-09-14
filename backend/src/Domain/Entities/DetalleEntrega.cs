using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-033/RN-034, 02-dominio.md §25, 04-base-datos.md §30: producto efectivamente entregado
// dentro de una entrega. 04-base-datos.md §30 no incluye ProductoId como columna propia (a
// diferencia de DetalleConsolidacion/DetallePedidoProveedor); el producto se alcanza siempre a
// través de DetallePedidoOrigen.Producto. Solo se crea/muta a través de Entrega (constructor y
// mutadores internal).
public sealed class DetalleEntrega
{
    public int Id { get; }
    public DetallePedidoProveedor DetallePedidoOrigen { get; }
    public int CantidadEntregada { get; }

    public Producto Producto => DetallePedidoOrigen.Producto;

    private readonly List<DistribucionEntrega> _distribuciones = new();
    public IReadOnlyList<DistribucionEntrega> Distribuciones => _distribuciones;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DetalleEntrega()
    {
    }
#pragma warning restore CS8618

    internal DetalleEntrega(int id, DetallePedidoProveedor detallePedidoOrigen, int cantidadEntregada)
    {
        if (detallePedidoOrigen is null)
            throw new ReglaDeNegocioException("Un detalle de entrega debe originarse en un detalle de pedido.");

        if (cantidadEntregada <= 0)
            throw new ReglaDeNegocioException("La cantidad entregada debe ser mayor que cero.");

        Id = id;
        DetallePedidoOrigen = detallePedidoOrigen;
        CantidadEntregada = cantidadEntregada;
    }

    internal DistribucionEntrega AgregarDistribucion(int id, Sede sede, int cantidad)
    {
        var distribucion = new DistribucionEntrega(id, sede, cantidad);
        _distribuciones.Add(distribucion);
        return distribucion;
    }
}
