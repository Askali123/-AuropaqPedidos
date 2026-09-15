using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class ProductoProveedorConfiguration : IEntityTypeConfiguration<ProductoProveedor>
{
    public void Configure(EntityTypeBuilder<ProductoProveedor> builder)
    {
        builder.ToTable("ProductosProveedores");

        builder.HasKey(pp => pp.Id);
        builder.Property(pp => pp.Id).ValueGeneratedNever();

        builder.Property(pp => pp.CodigoProveedor).IsRequired();
        builder.Property(pp => pp.DescripcionProveedor).IsRequired(false);
        builder.Property(pp => pp.CategoriaProveedor).IsRequired(false);
        builder.Property(pp => pp.UnidadProveedor).IsRequired(false);
        builder.Property(pp => pp.Activo).IsRequired();

        builder.HasOne(pp => pp.Producto)
            .WithMany()
            .HasForeignKey("ProductoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pp => pp.Proveedor)
            .WithMany()
            .HasForeignKey("ProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // 04-base-datos.md §15 "Restricción": "unicidad apropiada para evitar duplicar la misma
        // relación producto/proveedor" — un mismo Producto no puede tener dos relaciones con el
        // mismo Proveedor.
        builder.HasIndex("ProductoId", "ProveedorId").IsUnique();
    }
}
