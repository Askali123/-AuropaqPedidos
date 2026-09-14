using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuropaqPedidos.Infrastructure.Persistence.Configurations;

internal sealed class ContadorIdentificadorConfiguration : IEntityTypeConfiguration<ContadorIdentificador>
{
    public void Configure(EntityTypeBuilder<ContadorIdentificador> builder)
    {
        builder.ToTable("ContadorIdentificadores");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Valor).IsRequired();
    }
}
