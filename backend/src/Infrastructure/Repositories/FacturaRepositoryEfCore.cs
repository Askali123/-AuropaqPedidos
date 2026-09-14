using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class FacturaRepositoryEfCore : IFacturaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public FacturaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Factura? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(f => f.Id == id);

    public IReadOnlyList<Factura> ObtenerPorPedido(int pedidoProveedorId) =>
        Consulta().Where(f => EF.Property<int>(f, "PedidoProveedorId") == pedidoProveedorId).ToList();

    public bool ExisteNumeroFacturaParaProveedor(int proveedorId, string numeroFactura) =>
        _contexto.Facturas.Any(f =>
            EF.Property<int>(f, "ProveedorId") == proveedorId && f.NumeroFactura == numeroFactura);

    // Mismo criterio que los demás repositorios de agregado (Requisicion/Consolidacion/PedidoProveedor/Entrega).
    public void Guardar(Factura factura)
    {
        if (_contexto.Entry(factura).State == EntityState.Detached)
            _contexto.Facturas.Add(factura);

        _contexto.SaveChanges();
    }

    private IQueryable<Factura> Consulta() =>
        _contexto.Facturas
            .Include(f => f.Proveedor)
            .Include(f => f.PedidoProveedor).ThenInclude(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(f => f.Detalles).ThenInclude(d => d.DetallePedidoOrigen).ThenInclude(d => d.Producto);
}
