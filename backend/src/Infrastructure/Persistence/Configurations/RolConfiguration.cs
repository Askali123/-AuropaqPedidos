using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Nombre).IsRequired();

        // Descripcion: opcional. 04-base-datos.md §9.1 no documenta ninguna regla de unicidad
        // para Nombre (a diferencia de Usuario.Correo o Periodo Año+Mes) — mismo criterio ya
        // aplicado a Empresa.Nit, no se inventa aquí.
        builder.Property(r => r.Descripcion).IsRequired(false);

        builder.Property(r => r.Activo).IsRequired();
    }
}
