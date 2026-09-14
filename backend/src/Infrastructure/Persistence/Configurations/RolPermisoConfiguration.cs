using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("RolesPermisos");

        // Clave primaria compuesta (RolId, PermisoId) — mismo criterio ya aplicado a
        // UsuarioRol/UsuarioSede: el documento solo lista esos dos campos como identidad de la
        // relación; la propia PK garantiza "no duplicar" sin necesitar un índice único adicional.
        builder.HasKey("RolId", "PermisoId");

        builder.HasOne(rp => rp.Rol)
            .WithMany()
            .HasForeignKey("RolId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.Permiso)
            .WithMany()
            .HasForeignKey("PermisoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
