using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-005/RN-006: el periodo mensual y su ventana de solicitud son configurables, no hardcodeados.
public sealed class Periodo
{
    public int Id { get; private set; }
    public int Anio { get; private set; }
    public int Mes { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFin { get; private set; }
    public DateTime FechaInicioSolicitud { get; private set; }
    public DateTime FechaFinSolicitud { get; private set; }
    public string Estado { get; private set; }

    public Periodo(
        int id,
        int anio,
        int mes,
        DateTime fechaInicio,
        DateTime fechaFin,
        DateTime fechaInicioSolicitud,
        DateTime fechaFinSolicitud,
        string estado)
    {
        if (anio <= 0)
            throw new ReglaDeNegocioException("El año del periodo no es válido.");

        if (mes < 1 || mes > 12)
            throw new ReglaDeNegocioException("El mes del periodo debe estar entre 1 y 12.");

        if (fechaFin < fechaInicio)
            throw new ReglaDeNegocioException("La fecha de fin del periodo no puede ser anterior a la fecha de inicio.");

        if (fechaFinSolicitud < fechaInicioSolicitud)
            throw new ReglaDeNegocioException("La fecha de fin de la ventana de solicitud no puede ser anterior a la fecha de inicio.");

        if (string.IsNullOrWhiteSpace(estado))
            throw new ReglaDeNegocioException("El estado del periodo es obligatorio.");

        Id = id;
        Anio = anio;
        Mes = mes;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        FechaInicioSolicitud = fechaInicioSolicitud;
        FechaFinSolicitud = fechaFinSolicitud;
        Estado = estado;
    }

    // RN-006/RN-015: el envío de una requisición solo es válido dentro de la ventana de solicitud del periodo.
    public bool EstaDentroDeVentanaDeSolicitud(DateTime fecha) =>
        fecha >= FechaInicioSolicitud && fecha <= FechaFinSolicitud;
}
