using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class ProjetoConfiguration : IEntityTypeConfiguration<Projeto>
{
    public void Configure(EntityTypeBuilder<Projeto> builder)
    {
        builder.ToTable("projetos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.DataInicio).IsRequired();
        builder.Property(p => p.ClienteId).IsRequired();
        builder.Property(p => p.EtapaAtualId).IsRequired();

        builder.HasOne<Cliente>().WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Etapa>().WithMany().HasForeignKey(p => p.EtapaAtualId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EtapaAtualId);
    }
}
