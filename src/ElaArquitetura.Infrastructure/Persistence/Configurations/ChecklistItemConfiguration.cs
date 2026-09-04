using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("checklist_itens");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Descricao).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Concluido).IsRequired();

        builder.HasOne<Projeto>().WithMany().HasForeignKey(c => c.ProjetoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Etapa>().WithMany().HasForeignKey(c => c.EtapaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SubEtapa>().WithMany().HasForeignKey(c => c.SubEtapaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ProjetoId, c.EtapaId });
    }
}
