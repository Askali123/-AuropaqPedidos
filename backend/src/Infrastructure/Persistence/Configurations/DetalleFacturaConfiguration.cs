using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
{
    public void Configure(EntityTypeBuilder<DetalleFactura> builder)
    {
        builder.ToTable("DetallesFactura");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.CantidadFacturada).IsRequired();

        // Sin precisión explícita, EF Core generaría una advertencia de build (decimal sin
        // HasPrecision). PrecioUnitario es obligatorio en este incremento (decisión provisional
        // de alcance MVP: "se registra desde la factura del proveedor").
        builder.Property(d => d.PrecioUnitario).HasPrecision(18, 2);

        // Producto y Subtotal son propiedades calculadas de Domain: Producto pasa a través de
        // DetallePedidoOrigen.Producto (04-base-datos.md §27 no incluye ProductoId propio de esta
        // línea, mismo criterio que DetalleEntrega); Subtotal = CantidadFacturada × PrecioUnitario
        // (decisión provisional de alcance MVP). Ninguna se persiste.
        builder.Ignore(d => d.Producto);
        builder.Ignore(d => d.Subtotal);

        builder.HasOne(d => d.DetallePedidoOrigen)
            .WithMany()
            .HasForeignKey("DetallePedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
