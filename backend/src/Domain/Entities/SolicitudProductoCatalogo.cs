using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-024/RN-025/RN-026: solicitud de un producto no catalogado, sujeta a revisión
// (homologar / crear / rechazar), conservando siempre la información original solicitada.
public sealed class SolicitudProductoCatalogo
{
    public int Id { get; private set; }
    public Empresa Empresa { get; private set; }

    // UsuarioId queda como identificador simple porque la entidad Usuario todavía no existe en Domain (TASK-008 pendiente).
    public int UsuarioId { get; private set; }

    public string NombreSolicitado { get; private set; }
    public string? Descripcion { get; private set; }
    public string? Observacion { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public SolicitudProductoCatalogoEstado Estado { get; private set; }

    public Producto? ProductoResultante { get; private set; }
    public DateTime? FechaResolucion { get; private set; }
    public int? UsuarioResolucionId { get; private set; }
    public string? MotivoResolucion { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private SolicitudProductoCatalogo()
    {
    }
#pragma warning restore CS8618

    public SolicitudProductoCatalogo(
        int id,
        Empresa empresa,
        int usuarioId,
        string nombreSolicitado,
        DateTime fechaSolicitud,
        string? descripcion = null,
        string? observacion = null)
    {
        if (empresa is null)
            throw new ReglaDeNegocioException("Una solicitud de producto no catalogado debe pertenecer a una empresa.");

        if (string.IsNullOrWhiteSpace(nombreSolicitado))
            throw new ReglaDeNegocioException("El nombre solicitado del producto es obligatorio.");

        Id = id;
        Empresa = empresa;
        UsuarioId = usuarioId;
        NombreSolicitado = nombreSolicitado;
        Descripcion = descripcion;
        Observacion = observacion;
        FechaSolicitud = fechaSolicitud;
        Estado = SolicitudProductoCatalogoEstado.Pendiente;
    }

    public void Homologar(Producto productoExistente, int usuarioResolucionId, DateTime fechaResolucion, string? motivo = null)
    {
        if (productoExistente is null)
            throw new ReglaDeNegocioException("Homologar requiere un producto existente.");

        Resolver(SolicitudProductoCatalogoEstado.Homologado, productoExistente, usuarioResolucionId, fechaResolucion, motivo);
    }

    public void Crear(Producto productoNuevo, int usuarioResolucionId, DateTime fechaResolucion, string? motivo = null)
    {
        if (productoNuevo is null)
            throw new ReglaDeNegocioException("Crear requiere el producto oficial recién creado.");

        Resolver(SolicitudProductoCatalogoEstado.Creado, productoNuevo, usuarioResolucionId, fechaResolucion, motivo);
    }

    public void Rechazar(int usuarioResolucionId, DateTime fechaResolucion, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ReglaDeNegocioException("Rechazar una solicitud requiere un motivo.");

        Resolver(SolicitudProductoCatalogoEstado.Rechazado, productoResultante: null, usuarioResolucionId, fechaResolucion, motivo);
    }

    private void Resolver(
        SolicitudProductoCatalogoEstado nuevoEstado,
        Producto? productoResultante,
        int usuarioResolucionId,
        DateTime fechaResolucion,
        string? motivo)
    {
        if (Estado != SolicitudProductoCatalogoEstado.Pendiente)
            throw new ReglaDeNegocioException("Solamente una solicitud pendiente puede resolverse.");

        if (fechaResolucion < FechaSolicitud)
            throw new ReglaDeNegocioException("La fecha de resolución no puede ser anterior a la fecha de solicitud.");

        ProductoResultante = productoResultante;
        UsuarioResolucionId = usuarioResolucionId;
        FechaResolucion = fechaResolucion;
        MotivoResolucion = motivo;
        Estado = nuevoEstado;
    }
}
