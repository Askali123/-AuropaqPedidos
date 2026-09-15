using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        // Nullable: ver comentario en Auditoria.cs. Sin FK hacia Usuario (mismo criterio que
        // Consolidacion.UsuarioCreacionId): es un identificador simple, no una relación de
        // dominio que la Auditoria necesite navegar.
        builder.Property(a => a.UsuarioId).IsRequired(false);

        builder.Property(a => a.Entidad).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntidadId).IsRequired();
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Fecha).IsRequired();
        builder.Property(a => a.DatosAnteriores).IsRequired(false);
        builder.Property(a => a.DatosNuevos).IsRequired(false);

        // Consulta más probable: "qué le pasó a esta entidad" (p. ej. todos los registros de
        // Auditoria de un PedidoProveedor concreto).
        builder.HasIndex(a => new { a.Entidad, a.EntidadId });
    }
}
