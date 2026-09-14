using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nombre).IsRequired();
        builder.Property(p => p.Descripcion).IsRequired(false);

        // CodigoInterno: opcional, sin unicidad — 04-base-datos.md §12 dice "cuando el negocio
        // lo requiera"; la unicidad queda pendiente de confirmación del negocio.
        builder.Property(p => p.CodigoInterno).IsRequired(false);

        builder.Property(p => p.Activo).IsRequired();

        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey("CategoriaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UnidadMedida)
            .WithMany()
            .HasForeignKey("UnidadMedidaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
