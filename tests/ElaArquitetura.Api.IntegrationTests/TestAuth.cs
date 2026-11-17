using System.Net.Http.Headers;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Auth;

namespace ElaArquitetura.Api.IntegrationTests;

internal static class TestAuth
{
    // Os testes rodam num banco descartável (Testcontainers) e informam a usuária pela configuração
    // (CustomWebApplicationFactory); a senha é só de teste, nunca a real.
    public const string AdminEmail = "contato.elaarquitetura@gmail.com";
    public const string AdminSenha = "senha-somente-para-testes";

    public static async Task<HttpClient> ComoAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = AdminEmail, senha = AdminSenha });
        var body = await response.Content.ReadFromJsonAsync<LoginOutput>(TestJson.Options);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        return client;
    }
}
