using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nombre).IsRequired();

        // Nit/Contacto/Telefono/Correo: opcionales, sin restricción de unicidad — 04-base-datos.md
        // §14 no define esa decisión (mismo criterio ya usado para Empresa.Nit).
        builder.Property(p => p.Nit).IsRequired(false);
        builder.Property(p => p.Contacto).IsRequired(false);
        builder.Property(p => p.Telefono).IsRequired(false);
        builder.Property(p => p.Correo).IsRequired(false);

        builder.Property(p => p.Activo).IsRequired();
    }
}
