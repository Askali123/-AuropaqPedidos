using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.PedidosProveedor;

// Traduce las entidades de Domain a los DTOs de salida. Ningún caso de uso devuelve
// directamente una entidad de Domain (mismo criterio que RequisicionMapper/ConsolidacionMapper).
internal static class PedidoProveedorMapper
{
    public static PedidoProveedorResponse AResponse(PedidoProveedor pedido) => new(
        Id: pedido.Id,
        ConsolidacionId: pedido.Consolidacion.Id,
        ProveedorId: pedido.Proveedor.Id,
        NumeroPedido: pedido.NumeroPedido,
        UsuarioCreacionId: pedido.UsuarioCreacionId,
        FechaPedido: pedido.FechaPedido,
        FechaEntregaEstimada: pedido.FechaEntregaEstimada,
        Estado: pedido.Estado.ToString(),
        Observacion: pedido.Observacion,
        Detalles: pedido.Detalles.Select(d => ADetalleResponse(d, pedido.Consolidacion)).ToList());

    private static DetallePedidoProveedorResponse ADetalleResponse(DetallePedidoProveedor detalle, Consolidacion consolidacionOrigen)
    {
        // DetalleConsolidacionId no es una columna real (04-base-datos.md §27); se deriva aquí
        // solo para exponer la trazabilidad en la respuesta, buscando el DetalleConsolidacion
        // del mismo producto dentro de la consolidación de origen del pedido.
        var detalleConsolidacionId = consolidacionOrigen.Detalles
            .FirstOrDefault(dc => dc.Producto == detalle.Producto)?.Id;

        return new DetallePedidoProveedorResponse(
            Id: detalle.Id,
            ProductoId: detalle.Producto.Id,
            DetalleConsolidacionId: detalleConsolidacionId,
            CantidadNecesaria: detalle.CantidadNecesaria,
            CantidadPedida: detalle.CantidadPedida,
            PrecioUnitario: detalle.PrecioUnitario,
            CodigoProveedorUtilizado: detalle.CodigoProveedorUtilizado,
            Distribuciones: detalle.Distribuciones.Select(ADistribucionResponse).ToList());
    }

    private static DistribucionPedidoResponse ADistribucionResponse(DistribucionPedido distribucion) => new(
        Id: distribucion.Id,
        SedeId: distribucion.Sede.Id,
        Cantidad: distribucion.Cantidad);
}
