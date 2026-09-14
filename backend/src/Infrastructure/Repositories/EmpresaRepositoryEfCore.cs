using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class EmpresaRepositoryEfCore : IEmpresaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public EmpresaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Empresa? ObtenerPorId(int id) =>
        _contexto.Empresas.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Empresa> ObtenerTodas() =>
        _contexto.Empresas.OrderBy(e => e.Nombre).ToList();
}
