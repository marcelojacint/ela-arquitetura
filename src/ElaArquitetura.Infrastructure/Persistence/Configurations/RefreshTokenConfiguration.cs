using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElaArquitetura.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(rt => rt.CriadoEm).IsRequired();
        builder.Property(rt => rt.ExpiraEm).IsRequired();
        builder.Property(rt => rt.Revogado).IsRequired();

        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.HasOne<Funcionario>().WithMany().HasForeignKey(rt => rt.FuncionarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
