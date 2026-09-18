using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class DetallePedidoProveedorConfiguration : IEntityTypeConfiguration<DetallePedidoProveedor>
{
    public void Configure(EntityTypeBuilder<DetallePedidoProveedor> builder)
    {
        builder.ToTable("DetallesPedidoProveedor");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        // Fotografía del DetalleConsolidacion en el momento de agregarse (RN-028): no se
        // recalcula ni se referencia por FK (04-base-datos.md §27 no incluye DetalleConsolidacionId).
        builder.Property(d => d.CantidadNecesaria).IsRequired();
        builder.Property(d => d.CantidadPedida).IsRequired();

        // Sin precisión explícita, EF Core generaría una advertencia de build (decimal sin
        // HasPrecision). PrecioUnitario es opcional: 04-base-datos.md §27 no indica si es
        // obligatorio, y el precio puede no conocerse todavía al crear el detalle.
        builder.Property(d => d.PrecioUnitario).IsRequired(false).HasPrecision(18, 2);

        // TASK-105: fotografía de ProductoProveedor.CodigoProveedor en el momento de agregar el
        // detalle — no es una FK (mismo criterio que DistribucionEntrega.DireccionEntrega): si
        // el código se edita después en el catálogo, este valor no debe cambiar.
        builder.Property(d => d.CodigoProveedorUtilizado).IsRequired(false);

        // Propiedad calculada de Domain (SUM de Distribuciones.Cantidad): no se persiste.
        builder.Ignore(d => d.CantidadDistribuida);

        builder.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey("ProductoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.Distribuciones)
            .HasField("_distribuciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.Distribuciones)
            .WithOne()
            .HasForeignKey("DetallePedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
