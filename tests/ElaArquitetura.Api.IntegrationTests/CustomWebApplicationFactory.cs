using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("ela_arquitetura_test")
        .WithUsername("ela_arquitetura_test")
        .WithPassword("ela_arquitetura_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = "ElaArquitetura.Api.Tests",
                ["Jwt:Audience"] = "ElaArquitetura.App.Tests",
                ["Jwt:Key"] = "chave-simetrica-somente-para-testes-de-integracao-nao-usar-em-producao",
                ["Jwt:ExpirationMinutes"] = "60",
                // Os testes fazem dezenas de logins seguidos do mesmo "IP"; o limite real é testado à parte.
                ["RateLimiting:Login:PermitLimit"] = "100000"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
    }

    /// <summary>Id da usuária do sistema criada pelo DevSeed (a API não expõe esse cadastro).</summary>
    public async Task<Guid> ObterAdminIdAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ElaArquiteturaDbContext>();
        return await dbContext.Funcionarios.Where(f => f.Email == TestAuth.AdminEmail).Select(f => f.Id).SingleAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
