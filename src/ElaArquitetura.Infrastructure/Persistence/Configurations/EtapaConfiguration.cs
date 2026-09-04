using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

/// <summary>
/// As 6 etapas são dado fixo do processo (PRD seção 6) — nascem via seed, não são cadastradas pelo app.
/// </summary>
public class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> builder)
    {
        builder.ToTable("etapas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Ordem).IsRequired();
        builder.Property(e => e.Opcional).IsRequired();
        builder.Property(e => e.Final).IsRequired();

        builder.HasData(
            new { Id = EtapaSeedIds.CadastroDoCliente, Nome = "Cadastro do Cliente", Ordem = 1, Opcional = false, Final = false },
            new { Id = EtapaSeedIds.EstudosPreliminares, Nome = "Estudos Preliminares", Ordem = 2, Opcional = false, Final = false },
            new { Id = EtapaSeedIds.Anteprojeto, Nome = "Anteprojeto", Ordem = 3, Opcional = false, Final = false },
            new { Id = EtapaSeedIds.ProjetoExecutivo, Nome = "Projeto Executivo", Ordem = 4, Opcional = false, Final = false },
            new { Id = EtapaSeedIds.RelatorioDeObra, Nome = "Relatório de Obra", Ordem = 5, Opcional = true, Final = false },
            new { Id = EtapaSeedIds.ConclusaoEEntrega, Nome = "Conclusão e Entrega", Ordem = 6, Opcional = false, Final = true });
    }
}
