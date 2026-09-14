using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuariosRoles");

        // Clave primaria compuesta (UsuarioId, RolId) — mismo criterio ya aplicado a
        // UsuarioSede: el documento solo lista esos dos campos como identidad de la relación;
        // la propia PK garantiza "no duplicar" sin necesitar un índice único adicional.
        builder.HasKey("UsuarioId", "RolId");

        builder.HasOne(ur => ur.Usuario)
            .WithMany()
            .HasForeignKey("UsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Rol)
            .WithMany()
            .HasForeignKey("RolId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
