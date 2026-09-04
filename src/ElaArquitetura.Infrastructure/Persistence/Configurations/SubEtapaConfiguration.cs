using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

/// <summary>
/// Sub-etapas de "Estudos Preliminares" e "Projeto Executivo" (PRD seção 6) — também são seed fixo.
/// </summary>
public class SubEtapaConfiguration : IEntityTypeConfiguration<SubEtapa>
{
    public void Configure(EntityTypeBuilder<SubEtapa> builder)
    {
        builder.ToTable("sub_etapas");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Ordem).IsRequired();
        builder.Property(s => s.EtapaId).IsRequired();

        builder.HasOne<Etapa>().WithMany().HasForeignKey(s => s.EtapaId).OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000101"), EtapaId = EtapaSeedIds.EstudosPreliminares, Nome = "Briefing", Ordem = 1 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000102"), EtapaId = EtapaSeedIds.EstudosPreliminares, Nome = "Levantamento em Locação", Ordem = 2 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000103"), EtapaId = EtapaSeedIds.EstudosPreliminares, Nome = "Estudo de Layout", Ordem = 3 },

            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000201"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Executivo de Obra", Ordem = 1 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000202"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Detalhamento de Marcenaria", Ordem = 2 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000203"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Detalhamento de Marmoraria", Ordem = 3 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000204"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Memoriais Descritivos", Ordem = 4 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000205"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Imagens", Ordem = 5 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000206"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Maquete 3D", Ordem = 6 },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000207"), EtapaId = EtapaSeedIds.ProjetoExecutivo, Nome = "Render", Ordem = 7 });
    }
}
