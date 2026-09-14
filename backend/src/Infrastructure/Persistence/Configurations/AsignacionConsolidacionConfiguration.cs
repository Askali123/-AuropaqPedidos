using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class AsignacionConsolidacionConfiguration : IEntityTypeConfiguration<AsignacionConsolidacion>
{
    public void Configure(EntityTypeBuilder<AsignacionConsolidacion> builder)
    {
        builder.ToTable("AsignacionesConsolidacion");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Cantidad).IsRequired();

        // RN-029: referencia (cross-aggregate, igual que Requisicion -> Empresa/Periodo) al
        // detalle de requisición de origen. Nunca se modifica desde aquí (RN-028).
        builder.HasOne(a => a.DetalleRequisicionOrigen)
            .WithMany()
            .HasForeignKey("DetalleRequisicionOrigenId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // A2 (cierre de negocio 2026-09-11): un DetalleRequisicion no puede quedar asignado a más
        // de una Consolidacion nunca — salvaguarda de base de datos para la regla de negocio ya
        // aplicada en CrearConsolidacionUseCase (una Requisicion participa una sola vez).
        builder.HasIndex("DetalleRequisicionOrigenId").IsUnique();
    }
}
