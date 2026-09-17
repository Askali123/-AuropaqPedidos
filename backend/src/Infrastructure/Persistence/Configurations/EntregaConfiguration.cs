using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

// Entrega es el agregado raíz (mismo criterio que Requisicion/Consolidacion/PedidoProveedor).
// Detalles solo son alcanzables a través de esta configuración; no se expone ningún DbSet propio.
public sealed class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.ToTable("Entregas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // UsuarioCreacionId agregado 2026-09-17 (P2-2): mismo criterio que
        // RequisicionConfiguration/ConsolidacionConfiguration.
        builder.Property(e => e.UsuarioCreacionId).IsRequired();
        builder.Property(e => e.FechaEntrega).IsRequired();
        builder.Property(e => e.NumeroRemision).IsRequired();

        // Cierre documental 2026-09-11 (D-04/RN-046): estados operativos (REGISTRADA/ANULADA),
        // persistidos como string (mismo criterio que RequisicionConfiguration.Estado).
        builder.Property(e => e.Estado).IsRequired().HasConversion<string>();

        builder.Property(e => e.Observacion).IsRequired(false);

        builder.HasOne(e => e.PedidoProveedor)
            .WithMany()
            .HasForeignKey("PedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // D-09/RN-049: NumeroRemision único dentro del pedido.
        builder.HasIndex("PedidoProveedorId", nameof(Entrega.NumeroRemision)).IsUnique();

        // Entrega.Detalles es una propiedad de solo lectura respaldada por el campo privado
        // _detalles (encapsulamiento del agregado); se usa acceso por campo para que EF Core
        // pueda materializar/persistir la colección sin exponer un setter público.
        builder.Navigation(e => e.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.Detalles)
            .WithOne()
            .HasForeignKey("EntregaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
