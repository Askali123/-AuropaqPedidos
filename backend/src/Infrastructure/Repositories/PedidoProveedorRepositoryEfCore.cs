using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class PedidoProveedorRepositoryEfCore : IPedidoProveedorRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public PedidoProveedorRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public PedidoProveedor? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<PedidoProveedor> Listar(int? consolidacionId)
    {
        var consulta = Consulta();

        if (consolidacionId is not null)
            consulta = consulta.Where(p => p.Consolidacion.Id == consolidacionId);

        return consulta.OrderByDescending(p => p.FechaPedido).ToList();
    }

    // Consulta directa a DetallePedidoProveedor (sin pasar por el DbSet<PedidoProveedor>
    // público): necesaria para Facturación (TASK-046), que debe poder resolver un detalle por Id
    // sin conocer de antemano a qué pedido pertenece. Comparte el mismo DbContext con scope de
    // request que el resto de los repositorios, por lo que el identity map de EF Core devuelve
    // la misma instancia ya rastreada si el pedido dueño también fue cargado en esta operación.
    public DetallePedidoProveedor? ObtenerDetallePorId(int detalleId) =>
        _contexto.Set<DetallePedidoProveedor>()
            .Include(d => d.Producto)
            .FirstOrDefault(d => d.Id == detalleId);

    public bool ExisteNumeroPedidoParaProveedor(int proveedorId, string numeroPedido) =>
        _contexto.PedidosProveedor.Any(p =>
            EF.Property<int>(p, "ProveedorId") == proveedorId && p.NumeroPedido == numeroPedido);

    // Mismo criterio que RequisicionRepositoryEfCore.Guardar / ConsolidacionRepositoryEfCore.Guardar.
    public void Guardar(PedidoProveedor pedido)
    {
        if (_contexto.Entry(pedido).State == EntityState.Detached)
            _contexto.PedidosProveedor.Add(pedido);

        _contexto.SaveChanges();
    }

    private IQueryable<PedidoProveedor> Consulta() =>
        _contexto.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Consolidacion).ThenInclude(c => c.Periodo)
            .Include(p => p.Consolidacion).ThenInclude(c => c.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Consolidacion).ThenInclude(c => c.Detalles).ThenInclude(d => d.Asignaciones).ThenInclude(a => a.DetalleRequisicionOrigen)
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Detalles).ThenInclude(d => d.Distribuciones).ThenInclude(dist => dist.Sede);
}
