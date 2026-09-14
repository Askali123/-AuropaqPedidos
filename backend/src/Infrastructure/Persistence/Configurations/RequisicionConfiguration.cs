using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

// Requisicion es el agregado raíz (instrucción explícita). Detalles/Historial solo son
// alcanzables a través de esta configuración; no se expone ningún DbSet propio para ellos.
public sealed class RequisicionConfiguration : IEntityTypeConfiguration<Requisicion>
{
    public void Configure(EntityTypeBuilder<Requisicion> builder)
    {
        builder.ToTable("Requisiciones");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        // UsuarioCreacionId: identificador simple porque Usuario todavía no existe en Domain.
        builder.Property(r => r.UsuarioCreacionId).IsRequired();

        builder.Property(r => r.Estado).IsRequired().HasConversion<string>();
        builder.Property(r => r.FechaCreacion).IsRequired();
        builder.Property(r => r.FechaEnvio).IsRequired(false);

        // Propiedad calculada de Domain (Estado is Borrador or Devuelta): no se persiste.
        builder.Ignore(r => r.EsEditable);

        builder.HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey("EmpresaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Periodo)
            .WithMany()
            .HasForeignKey("PeriodoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // RN-007/ADR-011, 04-base-datos.md §17: "UNIQUE(EmpresaId, PeriodoId)".
        builder.HasIndex("EmpresaId", "PeriodoId").IsUnique();

        // Requisicion.Detalles es una propiedad de solo lectura respaldada por el campo
        // privado _detalles (encapsulamiento del agregado); se usa acceso por campo para
        // que EF Core pueda materializar/persistir la colección sin exponer un setter público.
        builder.Navigation(r => r.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Detalles)
            .WithOne()
            .HasForeignKey("RequisicionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Historial)
            .HasField("_historial")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Historial)
            .WithOne()
            .HasForeignKey("RequisicionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
