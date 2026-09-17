using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class ConsolidacionRepositoryEfCore : IConsolidacionRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public ConsolidacionRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Consolidacion? ObtenerPorId(int id) =>
        _contexto.Consolidaciones
            .Include(c => c.Periodo)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .Include(c => c.Detalles).ThenInclude(d => d.Asignaciones).ThenInclude(a => a.DetalleRequisicionOrigen)
            .FirstOrDefault(c => c.Id == id);

    public IReadOnlyList<Consolidacion> Listar(int? periodoId)
    {
        var consulta = _contexto.Consolidaciones
            .Include(c => c.Periodo)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .Include(c => c.Detalles).ThenInclude(d => d.Asignaciones).ThenInclude(a => a.DetalleRequisicionOrigen)
            .AsQueryable();

        if (periodoId is not null)
            consulta = consulta.Where(c => c.Periodo.Id == periodoId);

        return consulta.OrderByDescending(c => c.FechaCreacion).ToList();
    }

    // A2: consulta directa a AsignacionConsolidacion (sin pasar por el DbSet<Consolidacion>
    // público) — solo necesita el shadow FK hacia DetalleRequisicion, no el agregado completo.
    public IReadOnlyList<int> ObtenerIdsDetallesRequisicionYaConsolidados() =>
        _contexto.Set<AsignacionConsolidacion>()
            .Select(a => EF.Property<int>(a, "DetalleRequisicionOrigenId"))
            .Distinct()
            .ToList();

    // Mismo criterio que RequisicionRepositoryEfCore.Guardar: se asume una única instancia de
    // AuropaqPedidosDbContext por caso de uso (Scoped). Una Consolidacion recién creada con
    // `new` nunca fue rastreada (Detached) y debe agregarse.
    public void Guardar(Consolidacion consolidacion)
    {
        if (_contexto.Entry(consolidacion).State == EntityState.Detached)
            _contexto.Consolidaciones.Add(consolidacion);

        _contexto.SaveChanges();
    }
}
