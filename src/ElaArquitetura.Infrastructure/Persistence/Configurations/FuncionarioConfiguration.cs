using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Email).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Cargo).HasMaxLength(100).IsRequired();
        builder.Property(f => f.SenhaHash).IsRequired();
        builder.Property(f => f.Ativo).IsRequired();

        builder.HasIndex(f => f.Email).IsUnique();
    }
}
