using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.ToTable("entregas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.LinkDrive).HasMaxLength(500).IsRequired();
        builder.Property(e => e.DataEnvio).IsRequired();
        builder.Property(e => e.EnviadoParaWhatsapp).IsRequired();

        builder.HasOne<Projeto>().WithMany().HasForeignKey(e => e.ProjetoId).OnDelete(DeleteBehavior.Cascade);
    }
}
