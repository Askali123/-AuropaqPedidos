using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Entregas;

// Traduce las entidades de Domain a los DTOs de salida. Ningún caso de uso devuelve
// directamente una entidad de Domain (mismo criterio que los demás Mapper existentes).
internal static class EntregaMapper
{
    public static EntregaResponse AResponse(Entrega entrega) => new(
        Id: entrega.Id,
        PedidoProveedorId: entrega.PedidoProveedor.Id,
        UsuarioCreacionId: entrega.UsuarioCreacionId,
        FechaEntrega: entrega.FechaEntrega,
        NumeroRemision: entrega.NumeroRemision,
        Estado: entrega.Estado.ToString(),
        Observacion: entrega.Observacion,
        Detalles: entrega.Detalles.Select(ADetalleResponse).ToList());

    private static DetalleEntregaResponse ADetalleResponse(DetalleEntrega detalle) => new(
        Id: detalle.Id,
        DetallePedidoProveedorId: detalle.DetallePedidoOrigen.Id,
        ProductoId: detalle.Producto.Id,
        CantidadEntregada: detalle.CantidadEntregada,
        Distribuciones: detalle.Distribuciones.Select(ADistribucionResponse).ToList());

    private static DistribucionEntregaResponse ADistribucionResponse(DistribucionEntrega distribucion) => new(
        Id: distribucion.Id,
        SedeId: distribucion.Sede.Id,
        Cantidad: distribucion.Cantidad,
        DireccionEntrega: distribucion.DireccionEntrega,
        CiudadEntrega: distribucion.CiudadEntrega,
        ContactoEntrega: distribucion.ContactoEntrega);
}
