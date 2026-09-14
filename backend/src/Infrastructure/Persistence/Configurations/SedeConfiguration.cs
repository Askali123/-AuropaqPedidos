using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class SedeConfiguration : IEntityTypeConfiguration<Sede>
{
    public void Configure(EntityTypeBuilder<Sede> builder)
    {
        builder.ToTable("Sedes");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Nombre).IsRequired();

        // Campos opcionales: 02-dominio.md §5 deja pendiente la estructura exacta de Sede.
        builder.Property(s => s.Direccion).IsRequired(false);
        builder.Property(s => s.Ciudad).IsRequired(false);
        builder.Property(s => s.Departamento).IsRequired(false);
        builder.Property(s => s.Telefono).IsRequired(false);
        builder.Property(s => s.Contacto).IsRequired(false);

        builder.Property(s => s.Activo).IsRequired();

        // Empresa no mantiene una colección de Sede (decisión ya documentada en progreso.md);
        // la relación se navega únicamente desde Sede hacia Empresa.
        builder.HasOne(s => s.Empresa)
            .WithMany()
            .HasForeignKey("EmpresaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
