using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DistribucionPedidoConfiguration : IEntityTypeConfiguration<DistribucionPedido>
{
    public void Configure(EntityTypeBuilder<DistribucionPedido> builder)
    {
        builder.ToTable("DistribucionesPedido");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Cantidad).IsRequired();

        // Sede de cualquier empresa (PedidoProveedor no pertenece a una sola empresa).
        builder.HasOne(d => d.Sede)
            .WithMany()
            .HasForeignKey("SedeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
