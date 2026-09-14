namespace AuropaqPedidos.Application.Periodos.Dtos;

// Nombre y forma tomados de docs/05-api.md §16.3. "Estado" no viaja en el body documentado —
// ver CrearPeriodoUseCase (se fija en "ABIERTO" al crear, decisión explícita del usuario: RN-005
// exige el campo pero ningún documento define sus valores válidos ni quién los asigna).
public sealed record CrearPeriodoRequest(
    int Anio,
    int Mes,
    DateTime FechaInicio,
    DateTime FechaFin,
    DateTime FechaInicioSolicitud,
    DateTime FechaFinSolicitud);
