using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class ProjetoFuncionarioConfiguration : IEntityTypeConfiguration<ProjetoFuncionario>
{
    public void Configure(EntityTypeBuilder<ProjetoFuncionario> builder)
    {
        builder.ToTable("projeto_funcionarios");
        builder.HasKey(pf => new { pf.ProjetoId, pf.FuncionarioId });

        builder.Property(pf => pf.PapelNoProjeto).HasMaxLength(100);

        builder.HasOne<Projeto>().WithMany().HasForeignKey(pf => pf.ProjetoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(pf => pf.FuncionarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
