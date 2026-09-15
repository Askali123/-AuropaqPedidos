using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class SolicitudProductoCatalogoRepositoryEfCore : ISolicitudProductoCatalogoRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public SolicitudProductoCatalogoRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public SolicitudProductoCatalogo? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(s => s.Id == id);

    public IReadOnlyList<SolicitudProductoCatalogo> ObtenerPendientes() =>
        Consulta()
            .Where(s => s.Estado == SolicitudProductoCatalogoEstado.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToList();

    // Mismo patrón que EmpresaRepositoryEfCore.Guardar.
    public void Guardar(SolicitudProductoCatalogo solicitud)
    {
        if (_contexto.Entry(solicitud).State == EntityState.Detached)
            _contexto.SolicitudesProductoCatalogo.Add(solicitud);

        _contexto.SaveChanges();
    }

    private IQueryable<SolicitudProductoCatalogo> Consulta() =>
        _contexto.SolicitudesProductoCatalogo
            .Include(s => s.Empresa)
            .Include(s => s.ProductoResultante);
}
