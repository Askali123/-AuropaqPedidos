using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IPermisoRepository
{
    Permiso? ObtenerPorId(int id);

    IReadOnlyList<Permiso> ObtenerTodos();

    // RN-055/ADR-056: Codigo único GLOBAL — se comprueba en Application antes de construir el
    // Permiso (mismo criterio ya usado para Usuario.Correo y Periodo Año+Mes).
    bool ExisteParaCodigo(string codigo);

    void Guardar(Permiso permiso);
}
