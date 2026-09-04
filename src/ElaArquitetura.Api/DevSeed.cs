using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Api;

/// <summary>
/// Cria um funcionário padrão só em Development, para dar para testar o login (RF12)
/// sem precisar inserir dados manualmente. A senha nunca é hardcoded no banco/migration —
/// o hash é calculado em runtime com o IPasswordHasher configurado.
/// </summary>
internal static class DevSeed
{
    private const string EmailPadrao = "admin@elaarquitetura.com.br";
    private const string SenhaPadrao = "Trocar123!";

    public static async Task GarantirFuncionarioPadraoAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ElaArquiteturaDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await dbContext.Funcionarios.AnyAsync())
            return;

        var senhaHash = passwordHasher.Hash(SenhaPadrao);
        var funcionario = Funcionario.Criar("Administradora", EmailPadrao, "Proprietaria", senhaHash);

        dbContext.Funcionarios.Add(funcionario);
        await dbContext.SaveChangesAsync();
    }
}
