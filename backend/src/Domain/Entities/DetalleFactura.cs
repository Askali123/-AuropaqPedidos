using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// Decisión provisional de alcance MVP (Decisión 2 de "DECISIONES DE NEGOCIO PENDIENTES —
// FACTURACIÓN"): cada línea de factura corresponde a un DetallePedidoProveedor (no se relaciona
// directamente con Entrega en este incremento). Producto se alcanza a través de
// DetallePedidoOrigen.Producto (mismo criterio que DetalleEntrega.Producto). Solo se crea a
// través de Factura (constructor internal).
public sealed class DetalleFactura
{
    public int Id { get; }
    public DetallePedidoProveedor DetallePedidoOrigen { get; }
    public int CantidadFacturada { get; }
    public decimal PrecioUnitario { get; }

    public Producto Producto => DetallePedidoOrigen.Producto;

    // Decisión provisional de alcance MVP: Subtotal de línea = CantidadFacturada × PrecioUnitario.
    public decimal Subtotal => CantidadFacturada * PrecioUnitario;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DetalleFactura()
    {
    }
#pragma warning restore CS8618

    internal DetalleFactura(int id, DetallePedidoProveedor detallePedidoOrigen, int cantidadFacturada, decimal precioUnitario)
    {
        if (detallePedidoOrigen is null)
            throw new ReglaDeNegocioException("Un detalle de factura debe originarse en un detalle de pedido.");

        if (cantidadFacturada <= 0)
            throw new ReglaDeNegocioException("La cantidad facturada debe ser mayor que cero.");

        if (precioUnitario <= 0)
            throw new ReglaDeNegocioException("El precio unitario debe ser mayor que cero.");

        Id = id;
        DetallePedidoOrigen = detallePedidoOrigen;
        CantidadFacturada = cantidadFacturada;
        PrecioUnitario = precioUnitario;
    }
}
