using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class SolicitudProductoCatalogoConfiguration : IEntityTypeConfiguration<SolicitudProductoCatalogo>
{
    public void Configure(EntityTypeBuilder<SolicitudProductoCatalogo> builder)
    {
        builder.ToTable("SolicitudesProductoCatalogo");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        // UsuarioId: identificador simple porque Usuario todavía no existe en Domain.
        builder.Property(s => s.UsuarioId).IsRequired();

        builder.Property(s => s.NombreSolicitado).IsRequired();
        builder.Property(s => s.Descripcion).IsRequired(false);
        builder.Property(s => s.Observacion).IsRequired(false);
        builder.Property(s => s.FechaSolicitud).IsRequired();

        // Estado documentado en 04-base-datos.md §13 (Pendiente/Homologado/Creado/Rechazado).
        builder.Property(s => s.Estado).IsRequired().HasConversion<string>();

        builder.Property(s => s.FechaResolucion).IsRequired(false);
        builder.Property(s => s.UsuarioResolucionId).IsRequired(false);
        builder.Property(s => s.MotivoResolucion).IsRequired(false);

        builder.HasOne(s => s.Empresa)
            .WithMany()
            .HasForeignKey("EmpresaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ProductoResultante)
            .WithMany()
            .HasForeignKey("ProductoResultanteId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
