using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DistribucionEntregaConfiguration : IEntityTypeConfiguration<DistribucionEntrega>
{
    public void Configure(EntityTypeBuilder<DistribucionEntrega> builder)
    {
        builder.ToTable("DistribucionesEntrega");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Cantidad).IsRequired();

        // Snapshot histórico (RN-035/ADR-019): columnas propias, independientes de Sede.
        builder.Property(d => d.DireccionEntrega).IsRequired(false);
        builder.Property(d => d.CiudadEntrega).IsRequired(false);
        builder.Property(d => d.ContactoEntrega).IsRequired(false);

        builder.HasOne(d => d.Sede)
            .WithMany()
            .HasForeignKey("SedeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
