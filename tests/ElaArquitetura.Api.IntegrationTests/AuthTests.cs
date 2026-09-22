using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Auth;
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
    public async Task Login_com_senha_errada_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = TestAuth.AdminEmail, senha = "errada" });

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
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = TestAuth.AdminEmail, senha = TestAuth.AdminSenha });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginOutput>(TestJson.Options);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody!.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginBody.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }
}
