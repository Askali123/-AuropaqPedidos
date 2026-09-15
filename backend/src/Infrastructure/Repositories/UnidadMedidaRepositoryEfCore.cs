using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class UnidadMedidaRepositoryEfCore : IUnidadMedidaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public UnidadMedidaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public UnidadMedida? ObtenerPorId(int id) =>
        _contexto.UnidadesMedida.FirstOrDefault(u => u.Id == id);

    public IReadOnlyList<UnidadMedida> ObtenerTodas() =>
        _contexto.UnidadesMedida.OrderBy(u => u.Nombre).ToList();

    // Mismo patrón que EmpresaRepositoryEfCore.Guardar.
    public void Guardar(UnidadMedida unidadMedida)
    {
        if (_contexto.Entry(unidadMedida).State == EntityState.Detached)
            _contexto.UnidadesMedida.Add(unidadMedida);

        _contexto.SaveChanges();
    }
}
