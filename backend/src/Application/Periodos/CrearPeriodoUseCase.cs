using AuropaqPedidos.Application.Periodos.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Periodos;

// docs/05-api.md §16.3, RN-005/RN-006. El "Estado" del Periodo se fija en "ABIERTO" al crear:
// decisión explícita del usuario, no inventada aquí — RN-005 exige el campo como mínimo
// identificador del Periodo, pero ningún documento define sus valores válidos ni quién los
// asigna (ambigüedad reportada y resuelta por instrucción explícita, no por criterio propio).
// No se agrega ningún ciclo de vida adicional (quién lo pasa a "CERRADO", cuándo, etc.).
public sealed class CrearPeriodoUseCase
{
    private const string EstadoInicial = "ABIERTO";

    private readonly IPeriodoRepository _periodos;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearPeriodoUseCase(IPeriodoRepository periodos, IGeneradorDeIdentificadores ids)
    {
        _periodos = periodos;
        _ids = ids;
    }

    public PeriodoResponse Ejecutar(CrearPeriodoRequest request)
    {
        // 04-base-datos.md §16 / docs/05-api.md §16.3: "Año + Mes = único".
        if (_periodos.ExisteParaAnioYMes(request.Anio, request.Mes))
            throw new ReglaDeNegocioException("Ya existe un periodo para ese año y mes.");

        var periodo = new Periodo(
            _ids.Siguiente(),
            request.Anio,
            request.Mes,
            request.FechaInicio,
            request.FechaFin,
            request.FechaInicioSolicitud,
            request.FechaFinSolicitud,
            EstadoInicial);

        _periodos.Guardar(periodo);

        return new PeriodoResponse(
            periodo.Id,
            periodo.Anio,
            periodo.Mes,
            periodo.FechaInicio,
            periodo.FechaFin,
            periodo.FechaInicioSolicitud,
            periodo.FechaFinSolicitud,
            periodo.Estado);
    }
}
