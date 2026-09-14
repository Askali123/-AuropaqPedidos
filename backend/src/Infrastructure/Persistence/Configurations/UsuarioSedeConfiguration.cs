using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class UsuarioSedeConfiguration : IEntityTypeConfiguration<UsuarioSede>
{
    public void Configure(EntityTypeBuilder<UsuarioSede> builder)
    {
        builder.ToTable("UsuariosSedes");

        // Clave primaria compuesta (UsuarioId, SedeId) — mismo par que documenta
        // 04-base-datos.md §8/§restricciones como la identidad natural de la relación; no se
        // agrega un Id sustituto no documentado, y la propia PK ya garantiza "no duplicar
        // UsuarioId + SedeId" sin necesitar un índice único adicional.
        builder.HasKey("UsuarioId", "SedeId");

        // Ningún registro se referencia individualmente desde el negocio (misma razón que
        // HistorialRequisicion no expone Id en Domain); no se navega de vuelta desde
        // Usuario/Sede (mismo criterio ya usado en Sede-Empresa: no se mantiene una colección en
        // el lado "uno").
        builder.HasOne(us => us.Usuario)
            .WithMany()
            .HasForeignKey("UsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Sede)
            .WithMany()
            .HasForeignKey("SedeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
