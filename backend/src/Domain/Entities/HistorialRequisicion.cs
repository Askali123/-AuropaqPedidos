using AuropaqPedidos.Domain.Enums;

namespace AuropaqPedidos.Domain.Entities;

// RN-036/RN-037, ADR-016: conserva cada transición de estado de la requisición.
// Solo Requisicion puede crear entradas (constructor internal) para que el historial
// sea siempre un efecto de una transición real, nunca un dato fabricado externamente.
public sealed class HistorialRequisicion
{
    public RequisicionEstado EstadoAnterior { get; }
    public RequisicionEstado EstadoNuevo { get; }
    public int UsuarioId { get; }
    public DateTime Fecha { get; }
    public string? Comentario { get; }

    internal HistorialRequisicion(
        RequisicionEstado estadoAnterior,
        RequisicionEstado estadoNuevo,
        int usuarioId,
        DateTime fecha,
        string? comentario)
    {
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        UsuarioId = usuarioId;
        Fecha = fecha;
        Comentario = comentario;
    }
}
