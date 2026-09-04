using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Endereco).HasMaxLength(300);
        builder.Property(c => c.DataCadastro).IsRequired();
        builder.Property(c => c.Ativo).IsRequired();

        // Owned type (não conversor de valor) para que buscas por telefone
        // (c.Telefone.Numero) sejam traduzíveis em SQL.
        builder.OwnsOne(c => c.Telefone, telefone =>
        {
            telefone.Property(t => t.Numero)
                .HasColumnName("telefone")
                .HasMaxLength(20)
                .IsRequired();

            telefone.HasIndex(t => t.Numero);
        });

        builder.HasIndex(c => c.Nome);
    }
}
