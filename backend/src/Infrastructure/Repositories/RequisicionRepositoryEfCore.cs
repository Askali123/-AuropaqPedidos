using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class RequisicionRepositoryEfCore : IRequisicionRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public RequisicionRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Requisicion? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(r => r.Id == id);

    public Requisicion? ObtenerPorEmpresaYPeriodo(int empresaId, int periodoId) =>
        Consulta().FirstOrDefault(r =>
            EF.Property<int>(r, "EmpresaId") == empresaId && EF.Property<int>(r, "PeriodoId") == periodoId);

    public IReadOnlyList<Requisicion> ObtenerAprobadasPorPeriodo(int periodoId) =>
        Consulta()
            .Where(r => EF.Property<int>(r, "PeriodoId") == periodoId && r.Estado == RequisicionEstado.Aprobada)
            .ToList();

    public IReadOnlyList<Requisicion> ObtenerPorEmpresa(int empresaId) =>
        Consulta()
            .Where(r => EF.Property<int>(r, "EmpresaId") == empresaId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToList();

    public IReadOnlyList<Requisicion> ObtenerEnRevisionPorEmpresa(int empresaId) =>
        Consulta()
            .Where(r => EF.Property<int>(r, "EmpresaId") == empresaId && r.Estado == RequisicionEstado.EnRevision)
            .OrderBy(r => r.FechaEnvio)
            .ToList();

    // Se asume una única instancia de AuropaqPedidosDbContext por caso de uso (ciclo de vida
    // "Scoped" registrado en DI): un Requisicion recién creado con `new` nunca fue rastreado
    // (Detached) y debe agregarse; uno obtenido antes con ObtenerPorId/ObtenerPorEmpresaYPeriodo
    // ya está rastreado y sus cambios se detectan automáticamente al llamar SaveChanges.
    public void Guardar(Requisicion requisicion)
    {
        if (_contexto.Entry(requisicion).State == EntityState.Detached)
            _contexto.Requisiciones.Add(requisicion);

        _contexto.SaveChanges();
    }

    private IQueryable<Requisicion> Consulta() =>
        _contexto.Requisiciones
            .Include(r => r.Empresa)
            .Include(r => r.Periodo)
            .Include(r => r.Detalles).ThenInclude(d => d.Producto)
            .Include(r => r.Detalles).ThenInclude(d => d.Distribuciones).ThenInclude(dist => dist.Sede)
            .Include(r => r.Historial.OrderBy(h => EF.Property<int>(h, "Id")));
}
