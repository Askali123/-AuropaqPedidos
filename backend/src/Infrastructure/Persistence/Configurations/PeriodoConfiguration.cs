using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class PeriodoConfiguration : IEntityTypeConfiguration<Periodo>
{
    public void Configure(EntityTypeBuilder<Periodo> builder)
    {
        builder.ToTable("Periodos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Anio).IsRequired();
        builder.Property(p => p.Mes).IsRequired();
        builder.Property(p => p.FechaInicio).IsRequired();
        builder.Property(p => p.FechaFin).IsRequired();
        builder.Property(p => p.FechaInicioSolicitud).IsRequired();
        builder.Property(p => p.FechaFinSolicitud).IsRequired();

        // Texto libre: no hay valores definidos para el estado de Periodo (ambigüedad ya reportada).
        builder.Property(p => p.Estado).IsRequired();

        // 04-base-datos.md §16: "Debe existir como máximo un periodo para la combinación Año + Mes."
        builder.HasIndex(p => new { p.Anio, p.Mes }).IsUnique();
    }
}
