using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class EntregaRepositoryEfCore : IEntregaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public EntregaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Entrega? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Entrega> ObtenerPorPedido(int pedidoProveedorId) =>
        Consulta().Where(e => EF.Property<int>(e, "PedidoProveedorId") == pedidoProveedorId).ToList();

    // Mismo criterio que los demás repositorios de agregado (Requisicion/Consolidacion/PedidoProveedor).
    public void Guardar(Entrega entrega)
    {
        if (_contexto.Entry(entrega).State == EntityState.Detached)
            _contexto.Entregas.Add(entrega);

        _contexto.SaveChanges();
    }

    private IQueryable<Entrega> Consulta() =>
        _contexto.Entregas
            .Include(e => e.PedidoProveedor).ThenInclude(p => p.Proveedor)
            .Include(e => e.PedidoProveedor).ThenInclude(p => p.Consolidacion).ThenInclude(c => c.Periodo)
            .Include(e => e.PedidoProveedor).ThenInclude(p => p.Consolidacion).ThenInclude(c => c.Detalles).ThenInclude(d => d.Producto)
            .Include(e => e.PedidoProveedor).ThenInclude(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(e => e.Detalles).ThenInclude(d => d.DetallePedidoOrigen).ThenInclude(d => d.Producto)
            .Include(e => e.Detalles).ThenInclude(d => d.Distribuciones).ThenInclude(dist => dist.Sede);
}
