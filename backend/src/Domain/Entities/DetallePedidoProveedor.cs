using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-031, 02-dominio.md §22, 04-base-datos.md §27: un producto incluido en el pedido, con la
// diferencia explícita entre CantidadNecesaria (fotografía de la necesidad consolidada en el
// momento de agregar el detalle, nunca se recalcula) y CantidadPedida (lo que efectivamente se
// pide, puede diferir — RN-031). No existe columna DetalleConsolidacionId en el esquema
// documentado (04-base-datos.md §27 solo lista ProductoId); la trazabilidad hacia el
// DetalleConsolidacion de origen se resuelve indirectamente por (Producto + Consolidacion del
// pedido), válido porque una Consolidacion nunca mezcla dos DetalleConsolidacion del mismo
// producto (ver PedidoProveedor.AgregarDetalle). Solo se crea/muta a través de PedidoProveedor
// (constructor y mutadores internal).
public sealed class DetallePedidoProveedor
{
    public int Id { get; }
    public Producto Producto { get; }
    public int CantidadNecesaria { get; }
    public int CantidadPedida { get; private set; }
    public decimal? PrecioUnitario { get; private set; }

    private readonly List<DistribucionPedido> _distribuciones = new();
    public IReadOnlyList<DistribucionPedido> Distribuciones => _distribuciones;

    public int CantidadDistribuida => _distribuciones.Sum(d => d.Cantidad);

    // RN-065/D-18 (2026-09-17, incremento "Fase 6-9 en Frontend"): SUM(distribuciones) debe
    // coincidir con CantidadPedida antes de enviar — mismo criterio que
    // DetalleRequisicion.DistribucionCompleta (RN-011).
    public bool DistribucionCompleta => CantidadDistribuida == CantidadPedida;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DetallePedidoProveedor()
    {
    }
#pragma warning restore CS8618

    internal DetallePedidoProveedor(int id, Producto producto, int cantidadNecesaria, int cantidadPedida, decimal? precioUnitario)
    {
        if (producto is null)
            throw new ReglaDeNegocioException("Un detalle de pedido debe tener un producto.");

        if (cantidadPedida <= 0)
            throw new ReglaDeNegocioException("La cantidad pedida debe ser mayor que cero.");

        Id = id;
        Producto = producto;
        CantidadNecesaria = cantidadNecesaria;
        CantidadPedida = cantidadPedida;
        PrecioUnitario = precioUnitario;
    }

    internal DistribucionPedido AgregarDistribucion(int id, Sede sede, int cantidad)
    {
        var distribucion = new DistribucionPedido(id, sede, cantidad);
        _distribuciones.Add(distribucion);
        return distribucion;
    }
}
