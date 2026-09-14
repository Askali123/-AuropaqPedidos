using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DistribucionRequisicionConfiguration : IEntityTypeConfiguration<DistribucionRequisicion>
{
    public void Configure(EntityTypeBuilder<DistribucionRequisicion> builder)
    {
        builder.ToTable("DistribucionesRequisicion");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Cantidad).IsRequired();

        builder.HasOne(d => d.Sede)
            .WithMany()
            .HasForeignKey("SedeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
