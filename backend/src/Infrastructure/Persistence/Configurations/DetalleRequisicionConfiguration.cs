using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DetalleRequisicionConfiguration : IEntityTypeConfiguration<DetalleRequisicion>
{
    public void Configure(EntityTypeBuilder<DetalleRequisicion> builder)
    {
        builder.ToTable("DetallesRequisicion");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.CantidadSolicitada).IsRequired();
        builder.Property(d => d.Observacion).IsRequired(false);

        // Propiedades calculadas de Domain: no se persisten.
        builder.Ignore(d => d.CantidadDistribuida);
        builder.Ignore(d => d.DistribucionCompleta);

        builder.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey("ProductoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.Distribuciones)
            .HasField("_distribuciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.Distribuciones)
            .WithOne()
            .HasForeignKey("DetalleRequisicionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
