using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

// Factura es el agregado raíz de DetalleFactura (mismo criterio que Requisicion/Consolidacion/
// PedidoProveedor/Entrega). Detalles solo son alcanzables a través de esta configuración; no se
// expone ningún DbSet propio.
public sealed class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.NumeroFactura).IsRequired();
        // UsuarioCreacionId agregado 2026-09-17 (P2-2): mismo criterio que
        // RequisicionConfiguration/ConsolidacionConfiguration.
        builder.Property(f => f.UsuarioCreacionId).IsRequired();
        builder.Property(f => f.FechaFactura).IsRequired();

        // Sin precisión explícita, EF Core generaría una advertencia de build (decimal sin
        // HasPrecision). Impuestos se registra tal como lo indica la factura del proveedor (sin
        // cálculo de tasa — decisión provisional de alcance MVP).
        builder.Property(f => f.Impuestos).HasPrecision(18, 2);

        // Subtotal y Total son propiedades calculadas de Domain (decisión provisional de alcance
        // MVP: Subtotal = SUM(Detalles.Subtotal), Total = Subtotal + Impuestos). No se persisten.
        builder.Ignore(f => f.Subtotal);
        builder.Ignore(f => f.Total);

        // Cierre documental 2026-09-11 (D-05/RN-047): estados operativos (REGISTRADA/ANULADA),
        // sin ciclo contable (RN-038), persistidos como string (mismo criterio que
        // RequisicionConfiguration.Estado).
        builder.Property(f => f.Estado).IsRequired().HasConversion<string>();

        builder.Property(f => f.Observacion).IsRequired(false);

        builder.HasOne(f => f.Proveedor)
            .WithMany()
            .HasForeignKey("ProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // D-09/RN-049: NumeroFactura único dentro del proveedor.
        builder.HasIndex("ProveedorId", nameof(Factura.NumeroFactura)).IsUnique();

        // Decisión provisional de alcance MVP: PedidoProveedor 1 ─── N Facturas (Decisión 1 de
        // "DECISIONES DE NEGOCIO PENDIENTES — FACTURACIÓN" sigue pendiente para el caso general
        // de facturación consolidada N:N). NO se agrega relación hacia Entrega en este incremento.
        builder.HasOne(f => f.PedidoProveedor)
            .WithMany()
            .HasForeignKey("PedidoProveedorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Factura.Detalles es una propiedad de solo lectura respaldada por el campo privado
        // _detalles (encapsulamiento del agregado); se usa acceso por campo para que EF Core
        // pueda materializar/persistir la colección sin exponer un setter público.
        builder.Navigation(f => f.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Detalles)
            .WithOne()
            .HasForeignKey("FacturaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
