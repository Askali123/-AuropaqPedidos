using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IPeriodoRepository
{
    Periodo? ObtenerPorId(int id);

    // Soporta el selector de periodo del Frontend (TASK: habilitar consultas para Requisiciones).
    IReadOnlyList<Periodo> ObtenerTodos();

    // docs/05-api.md §16.3 / 04-base-datos.md §16: "Año + Mes = único". Se comprueba en
    // Application (mismo criterio ya usado para NumeroPedido/NumeroFactura) en vez de depender
    // de capturar la violación del índice único de SQL Server.
    bool ExisteParaAnioYMes(int anio, int mes);

    void Guardar(Periodo periodo);
}
