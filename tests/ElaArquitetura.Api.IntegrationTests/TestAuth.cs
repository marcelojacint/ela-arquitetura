using System.Net.Http.Headers;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Auth;

namespace ElaArquitetura.Api.IntegrationTests;

internal static class TestAuth
{
    public const string AdminEmail = "admin@elaarquitetura.com.br";
    public const string AdminSenha = "Trocar123!";

    public static async Task<HttpClient> ComoAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = AdminEmail, senha = AdminSenha });
        var body = await response.Content.ReadFromJsonAsync<LoginOutput>(TestJson.Options);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        return client;
    }
}
