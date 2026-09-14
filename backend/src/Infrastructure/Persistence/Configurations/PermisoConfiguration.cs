using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permisos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Codigo: único GLOBAL (RN-055/ADR-056, decisión de hardening previa a TASK-013) —
        // necesita una longitud acotada para poder indexarse en SQL Server (mismo motivo que
        // Usuario.Correo/NumeroPedido/NumeroFactura en otras configuraciones de este proyecto).
        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.Nombre).IsRequired();

        // Descripcion: opcional. 04-base-datos.md §9.2 no documenta ninguna regla de unicidad
        // para Nombre (a diferencia de Codigo) — mismo criterio ya aplicado a Rol.Nombre.
        builder.Property(p => p.Descripcion).IsRequired(false);
    }
}
