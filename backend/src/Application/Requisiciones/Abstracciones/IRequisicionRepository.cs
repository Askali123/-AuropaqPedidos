using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IRequisicionRepository
{
    Requisicion? ObtenerPorId(int id);

    // RN-007/ADR-011: soporta el "crear o recuperar" de TASK-022 sin que Domain conozca la unicidad Empresa+Periodo.
    Requisicion? ObtenerPorEmpresaYPeriodo(int empresaId, int periodoId);

    // RN-027/TASK-036: la consolidación de un periodo se construye a partir de todas las
    // requisiciones APROBADAS de ese periodo (de cualquier empresa).
    IReadOnlyList<Requisicion> ObtenerAprobadasPorPeriodo(int periodoId);

    // TASK-031/032, 05-api.md §17.1/§54.6: "mis requisiciones" — alcance por empresa (mismo
    // criterio ya establecido para enviar/aprobar, RN-058), sin filtro de estado.
    IReadOnlyList<Requisicion> ObtenerPorEmpresa(int empresaId);

    // TASK-031, 05-api.md §24.1/§54.6: bandeja de revisión — mismo alcance por empresa.
    IReadOnlyList<Requisicion> ObtenerEnRevisionPorEmpresa(int empresaId);

    void Guardar(Requisicion requisicion);
}
