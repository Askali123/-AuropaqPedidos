using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

// Consolidacion es el agregado raíz (mismo criterio que Requisicion). Detalles solo son
// alcanzables a través de esta configuración; no se expone ningún DbSet propio para ellos.
public sealed class ConsolidacionConfiguration : IEntityTypeConfiguration<Consolidacion>
{
    public void Configure(EntityTypeBuilder<Consolidacion> builder)
    {
        builder.ToTable("Consolidaciones");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        // UsuarioCreacionId: identificador simple porque Usuario todavía no existe en Domain.
        builder.Property(c => c.UsuarioCreacionId).IsRequired();

        // Texto libre: 04-base-datos.md §23 no define valores para Estado de Consolidacion
        // (a diferencia de RequisicionEstado). Mismo criterio ya usado para Periodo.Estado.
        builder.Property(c => c.Estado).IsRequired();

        builder.Property(c => c.FechaCreacion).IsRequired();
        builder.Property(c => c.Observacion).IsRequired(false);

        builder.HasOne(c => c.Periodo)
            .WithMany()
            .HasForeignKey("PeriodoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Consolidacion.Detalles es una propiedad de solo lectura respaldada por el campo
        // privado _detalles (encapsulamiento del agregado); se usa acceso por campo para que
        // EF Core pueda materializar/persistir la colección sin exponer un setter público.
        builder.Navigation(c => c.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Detalles)
            .WithOne()
            .HasForeignKey("ConsolidacionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
