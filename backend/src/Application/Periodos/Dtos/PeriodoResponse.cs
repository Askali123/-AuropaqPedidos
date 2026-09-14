namespace AuropaqPedidos.Application.Periodos.Dtos;

// Información necesaria para que el Frontend seleccione el periodo de una Requisición
// (docs/05-api.md §54.6). Incluye las fechas de la ventana de solicitud tal cual las expone el
// dominio (Periodo.EstaDentroDeVentanaDeSolicitud) para que el Frontend pueda mostrar si un
// periodo está actualmente abierto — sin que el backend decida aquí un filtro de "solo
// periodos abiertos" que ninguna regla de negocio define para esta consulta (esa validación ya
// existe, y solo ahí, en Requisicion.Enviar()).
public sealed record PeriodoResponse(
    int Id,
    int Anio,
    int Mes,
    DateTime FechaInicio,
    DateTime FechaFin,
    DateTime FechaInicioSolicitud,
    DateTime FechaFinSolicitud,
    string Estado);
