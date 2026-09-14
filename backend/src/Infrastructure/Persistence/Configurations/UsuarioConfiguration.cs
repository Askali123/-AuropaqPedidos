using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Apellido).IsRequired(false).HasMaxLength(200);

        // Correo: único GLOBAL (decisión confirmada explícitamente, no por empresa) — necesita
        // una longitud acotada para poder indexarse en SQL Server (mismo motivo que
        // NumeroPedido/NumeroFactura/NumeroRemision en otras configuraciones de este proyecto).
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Correo).IsUnique();

        // TASK-015: PasswordHasher<T> (PBKDF2, formato V3) produce una cadena base64 de longitud
        // fija (~84 caracteres); 256 deja margen sin necesitar nvarchar(max) para un valor que
        // nunca se indexa ni se busca por igualdad.
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);

        builder.Property(u => u.Activo).IsRequired();
        builder.Property(u => u.FechaCreacion).IsRequired();
        builder.Property(u => u.FechaActualizacion).IsRequired();

        // Un usuario pertenece a exactamente una empresa; Empresa no mantiene una colección de
        // Usuario (mismo criterio ya documentado para Empresa-Sede).
        builder.HasOne(u => u.Empresa)
            .WithMany()
            .HasForeignKey("EmpresaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
