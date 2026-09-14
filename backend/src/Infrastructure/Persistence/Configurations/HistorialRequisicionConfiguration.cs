using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class HistorialRequisicionConfiguration : IEntityTypeConfiguration<HistorialRequisicion>
{
    public void Configure(EntityTypeBuilder<HistorialRequisicion> builder)
    {
        builder.ToTable("HistorialRequisicion");

        // HistorialRequisicion no expone un Id en Domain (nunca se referencia individualmente
        // desde el negocio). Se usa una clave shadow autogenerada solo para satisfacer el
        // requisito de EF Core de tener una clave primaria; no modifica la entidad de Domain.
        builder.Property<int>("Id").ValueGeneratedOnAdd();
        builder.HasKey("Id");

        builder.Property(h => h.EstadoAnterior).IsRequired().HasConversion<string>();
        builder.Property(h => h.EstadoNuevo).IsRequired().HasConversion<string>();
        builder.Property(h => h.UsuarioId).IsRequired();
        builder.Property(h => h.Fecha).IsRequired();
        builder.Property(h => h.Comentario).IsRequired(false);
    }
}
