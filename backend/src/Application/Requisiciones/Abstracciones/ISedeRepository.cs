using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface ISedeRepository
{
    Sede? ObtenerPorId(int id);

    // Soporta el selector de sede del Frontend (TASK: habilitar consultas para Requisiciones).
    IReadOnlyList<Sede> ObtenerPorEmpresa(int empresaId);
}
