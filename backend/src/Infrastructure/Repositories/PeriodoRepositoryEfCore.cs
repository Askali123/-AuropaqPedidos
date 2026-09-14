using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class PeriodoRepositoryEfCore : IPeriodoRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public PeriodoRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Periodo? ObtenerPorId(int id) =>
        _contexto.Periodos.FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<Periodo> ObtenerTodos() =>
        _contexto.Periodos.OrderBy(p => p.Anio).ThenBy(p => p.Mes).ToList();

    public bool ExisteParaAnioYMes(int anio, int mes) =>
        _contexto.Periodos.Any(p => p.Anio == anio && p.Mes == mes);

    // Mismo patrón que RequisicionRepositoryEfCore.Guardar: un Periodo recién creado con `new`
    // nunca fue rastreado (Detached) y debe agregarse explícitamente.
    public void Guardar(Periodo periodo)
    {
        if (_contexto.Entry(periodo).State == EntityState.Detached)
            _contexto.Periodos.Add(periodo);

        _contexto.SaveChanges();
    }
}
