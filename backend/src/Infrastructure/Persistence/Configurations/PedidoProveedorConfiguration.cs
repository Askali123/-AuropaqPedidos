using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

// PedidoProveedor es el agregado raíz (mismo criterio que Requisicion/Consolidacion). Detalles
// solo son alcanzables a través de esta configuración; no se expone ningún DbSet propio.
public sealed class PedidoProveedorConfiguration : IEntityTypeConfiguration<PedidoProveedor>
{
    public void Configure(EntityTypeBuilder<PedidoProveedor> builder)
    {
        builder.ToTable("PedidosProveedor");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.NumeroPedido).IsRequired();
        // UsuarioCreacionId agregado 2026-09-17 (P2-2): mismo criterio que
        // RequisicionConfiguration/ConsolidacionConfiguration.
        builder.Property(p => p.UsuarioCreacionId).IsRequired();
        builder.Property(p => p.FechaPedido).IsRequired();
        builder.Property(p => p.FechaEntregaEstimada).IsRequired(false);

        // Cierre documental 2026-09-11 (D-03/RN-043): ciclo de estados cerrado, persistido como
        // string (mismo criterio que RequisicionConfiguration.Estado).
        builder.Property(p => p.Estado).IsRequired().HasConversion<string>();

        builder.Property(p => p.Observacion).IsRequired(false);

        builder.HasOne(p => p.Consolidacion)
            .WithMany()
            .HasForeignKey("ConsolidacionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Proveedor)
            .WithMany()
            .HasForeignKey("ProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // D-09/RN-049: NumeroPedido único dentro del proveedor.
        builder.HasIndex("ProveedorId", nameof(PedidoProveedor.NumeroPedido)).IsUnique();

        // PedidoProveedor.Detalles es una propiedad de solo lectura respaldada por el campo
        // privado _detalles (encapsulamiento del agregado); se usa acceso por campo para que
        // EF Core pueda materializar/persistir la colección sin exponer un setter público.
        builder.Navigation(p => p.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Detalles)
            .WithOne()
            .HasForeignKey("PedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
