using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DetalleEntregaConfiguration : IEntityTypeConfiguration<DetalleEntrega>
{
    public void Configure(EntityTypeBuilder<DetalleEntrega> builder)
    {
        builder.ToTable("DetallesEntrega");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.CantidadEntregada).IsRequired();

        // Producto es una propiedad calculada (pasa a través de DetallePedidoOrigen.Producto);
        // 04-base-datos.md §30 no incluye ProductoId como columna propia de DetalleEntrega.
        builder.Ignore(d => d.Producto);

        builder.HasOne(d => d.DetallePedidoOrigen)
            .WithMany()
            .HasForeignKey("DetallePedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.Distribuciones)
            .HasField("_distribuciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.Distribuciones)
            .WithOne()
            .HasForeignKey("DetalleEntregaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
