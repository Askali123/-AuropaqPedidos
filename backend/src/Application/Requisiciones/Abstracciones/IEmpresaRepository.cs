using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IEmpresaRepository
{
    Empresa? ObtenerPorId(int id);

    // Soporta el selector de empresa del Frontend (TASK: habilitar consultas para Requisiciones).
    IReadOnlyList<Empresa> ObtenerTodas();
}
