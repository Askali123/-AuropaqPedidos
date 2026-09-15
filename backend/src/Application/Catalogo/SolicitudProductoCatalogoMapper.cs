using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017. Mismo patrón que RequisicionMapper/PedidoProveedorMapper: evita repetir el mapeo en
// cada caso de uso de SolicitudProductoCatalogo.
internal static class SolicitudProductoCatalogoMapper
{
    public static SolicitudProductoCatalogoResponse AResponse(SolicitudProductoCatalogo solicitud) =>
        new(
            solicitud.Id,
            solicitud.Empresa.Id,
            solicitud.UsuarioId,
            solicitud.NombreSolicitado,
            solicitud.Descripcion,
            solicitud.Observacion,
            solicitud.Estado.ToString(),
            solicitud.ProductoResultante?.Id,
            solicitud.FechaSolicitud,
            solicitud.FechaResolucion,
            solicitud.UsuarioResolucionId,
            solicitud.MotivoResolucion);
}
