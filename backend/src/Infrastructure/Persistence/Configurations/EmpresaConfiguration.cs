using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Nombre).IsRequired();

        // Nit: opcional, sin restricción de unicidad — 04-base-datos.md §5.1 deja esa decisión
        // pendiente de confirmación del negocio. No se inventa aquí.
        builder.Property(e => e.Nit).IsRequired(false);

        builder.Property(e => e.Activo).IsRequired();
    }
}
