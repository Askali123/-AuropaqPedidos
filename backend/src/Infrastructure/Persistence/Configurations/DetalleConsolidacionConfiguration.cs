using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DetalleConsolidacionConfiguration : IEntityTypeConfiguration<DetalleConsolidacion>
{
    public void Configure(EntityTypeBuilder<DetalleConsolidacion> builder)
    {
        builder.ToTable("DetallesConsolidacion");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        // Propiedad calculada de Domain (SUM de Asignaciones.Cantidad): no se persiste.
        builder.Ignore(d => d.CantidadNecesaria);

        builder.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey("ProductoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.Asignaciones)
            .HasField("_asignaciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.Asignaciones)
            .WithOne()
            .HasForeignKey("DetalleConsolidacionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
