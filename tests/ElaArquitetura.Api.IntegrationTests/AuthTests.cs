using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AuthTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_com_credenciais_validas_deve_retornar_token_e_refresh_token()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = TestAuth.AdminEmail, senha = TestAuth.AdminSenha });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginOutput>(TestJson.Options);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    [Fact]
    public async Task Login_deve_ignorar_maiusculas_e_espacos_no_email()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "  " + TestAuth.AdminEmail.ToUpperInvariant() + " ", senha = TestAuth.AdminSenha });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_com_senha_errada_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = TestAuth.AdminEmail, senha = "errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_com_email_inexistente_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "ninguem@teste.com", senha = "qualquer" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_protegido_sem_token_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_token_deve_gerar_novo_par_e_revogar_o_antigo()
    {
        var client = _factory.CreateClient();
        var loginBody = await LoginAsync(client);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Reuso_de_refresh_token_revogado_deve_encerrar_todas_as_sessoes()
    {
        var client = _factory.CreateClient();
        var loginBody = await LoginAsync(client);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });
        var novoPar = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenOutput>(TestJson.Options);

        // Alguém reaproveita o token antigo (já trocado): isso derruba também o token novo.
        await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });

        var comTokenNovo = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = novoPar!.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, comTokenNovo.StatusCode);
    }

    [Fact]
    public async Task Logout_deve_revogar_o_refresh_token()
    {
        var client = _factory.CreateClient();
        var loginBody = await LoginAsync(client);

        var logout = await client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = loginBody.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Login_deve_ser_limitado_por_ip()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Login:PermitLimit"] = "2",
                ["RateLimiting:Login:WindowSeconds"] = "60"
            })));
        var client = factory.CreateClient();
        var request = new { email = TestAuth.AdminEmail, senha = "errada" };

        await client.PostAsJsonAsync("/api/auth/login", request);
        await client.PostAsJsonAsync("/api/auth/login", request);
        var terceira = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);
    }

    [Fact]
    public void Api_nao_deve_subir_sem_chave_jwt_valida()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "curta"
            })));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    private static async Task<LoginOutput> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = TestAuth.AdminEmail, senha = TestAuth.AdminSenha });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginOutput>(TestJson.Options))!;
    }
}
