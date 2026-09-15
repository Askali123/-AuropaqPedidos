using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-056, 04-base-datos.md §33: registro técnico/operativo de acciones relevantes, distinto
// del historial de negocio (p. ej. HistorialRequisicion, que ya cumple ese rol para Requisicion
// — 05-api.md §23 — y por eso no se duplica aquí). UsuarioId es nullable: los módulos de Fase
// 6-9 (PedidoProveedor/Entrega) todavía no exigen autenticación real (ver progreso.md, "punto 8"
// no extendido a esas fases todavía) y pueden no tener un actor identificado en el momento del
// registro.
public sealed class Auditoria
{
    public int Id { get; }
    public int? UsuarioId { get; }
    public string Entidad { get; }
    public int EntidadId { get; }
    public string Accion { get; }
    public DateTime Fecha { get; }
    public string? DatosAnteriores { get; }
    public string? DatosNuevos { get; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna regla de
    // negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Auditoria()
    {
    }
#pragma warning restore CS8618

    public Auditoria(
        int id, int? usuarioId, string entidad, int entidadId, string accion, DateTime fecha,
        string? datosAnteriores = null, string? datosNuevos = null)
    {
        if (string.IsNullOrWhiteSpace(entidad))
            throw new ReglaDeNegocioException("La entidad auditada es obligatoria.");

        if (string.IsNullOrWhiteSpace(accion))
            throw new ReglaDeNegocioException("La acción auditada es obligatoria.");

        Id = id;
        UsuarioId = usuarioId;
        Entidad = entidad;
        EntidadId = entidadId;
        Accion = accion;
        Fecha = fecha;
        DatosAnteriores = datosAnteriores;
        DatosNuevos = datosNuevos;
    }
}
