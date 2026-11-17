using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ElaArquitetura.Api;

/// <summary>
/// Garante, a cada inicialização e em qualquer ambiente, que a usuária do sistema existe com os dados
/// da configuração "UsuariaInicial". A configuração é a fonte da verdade: para trocar e-mail ou senha,
/// basta alterar a configuração e reiniciar a API. Quando a senha muda, todas as sessões abertas são encerradas.
/// O hash é calculado em runtime; a senha nunca vai para o banco nem para migrations.
/// </summary>
internal static class UsuariaInicialSeed
{
    public static async Task GarantirAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ElaArquiteturaDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var opcoes = scope.ServiceProvider.GetRequiredService<IOptions<UsuariaInicialOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(UsuariaInicialSeed));

        // A usuária é a única com senha (os demais funcionários são só cadastro de consulta).
        var usuaria = await dbContext.Funcionarios
            .Where(f => f.SenhaHash != null)
            .OrderByDescending(f => f.Ativo)
            .FirstOrDefaultAsync(cancellationToken);

        if (!opcoes.Configurada)
        {
            if (usuaria is null)
                logger.LogWarning(
                    "Nenhuma usuária cadastrada e a seção {Secao} não está configurada: ninguém consegue fazer login. " +
                    "Defina UsuariaInicial__Email e UsuariaInicial__Senha.", UsuariaInicialOptions.SectionName);
            return;
        }

        var email = Funcionario.NormalizarEmail(opcoes.Email);

        var emailEmUso = await dbContext.Funcionarios.AnyAsync(
            f => f.Ativo && f.Email == email && (usuaria == null || f.Id != usuaria.Id), cancellationToken);
        if (emailEmUso)
            throw new InvalidOperationException(
                $"O e-mail configurado em {UsuariaInicialOptions.SectionName}:Email já pertence a um funcionário cadastrado.");

        if (usuaria is null)
        {
            var nova = Funcionario.Criar(opcoes.Nome.Trim(), email, opcoes.Cargo, passwordHasher.Hash(opcoes.Senha));
            GarantirValida(nova);

            dbContext.Funcionarios.Add(nova);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Usuária do sistema criada a partir da configuração.");
            return;
        }

        var senhaMudou = !passwordHasher.Verificar(opcoes.Senha, usuaria.SenhaHash);
        var nome = opcoes.Nome.Trim();
        if (!senhaMudou && usuaria.Email == email && usuaria.Nome == nome && usuaria.Ativo)
            return;

        usuaria.RedefinirAcesso(nome, email, senhaMudou ? passwordHasher.Hash(opcoes.Senha) : usuaria.SenhaHash!);
        GarantirValida(usuaria);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (senhaMudou)
        {
            await dbContext.RefreshTokens
                .Where(rt => rt.FuncionarioId == usuaria.Id && !rt.Revogado)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revogado, true), cancellationToken);
        }

        logger.LogInformation("Dados de acesso da usuária atualizados a partir da configuração (senha alterada: {SenhaMudou}).", senhaMudou);
    }

    private static void GarantirValida(Funcionario funcionario)
    {
        if (!funcionario.IsValid)
            throw new InvalidOperationException(
                "Configuração da usuária inválida: " + string.Join(" ", funcionario.Notifications.Select(n => n.Mensagem)));
    }
}
