using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Funcionarios;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class FuncionariosTests
{
    private readonly CustomWebApplicationFactory _factory;

    public FuncionariosTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Fluxo_completo_criar_obter_atualizar_desativar()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var email = $"func.{Guid.NewGuid():N}@teste.com";

        var criarResponse = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "Funcionaria Integracao",
            email,
            cargo = "Estagiaria",
            senha = "SenhaForte123"
        });
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var funcionario = await criarResponse.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);

        var obterResponse = await client.GetAsync($"/api/funcionarios/{funcionario!.Id}");
        Assert.Equal(HttpStatusCode.OK, obterResponse.StatusCode);

        var atualizarResponse = await client.PutAsJsonAsync($"/api/funcionarios/{funcionario.Id}", new
        {
            nome = "Funcionaria Atualizada",
            email,
            cargo = "Arquiteta Junior"
        });
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var desativarResponse = await client.DeleteAsync($"/api/funcionarios/{funcionario.Id}");
        Assert.Equal(HttpStatusCode.NoContent, desativarResponse.StatusCode);
    }

    [Fact]
    public async Task Criar_funcionario_com_senha_curta_deve_retornar_400()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var response = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "X",
            email = $"{Guid.NewGuid():N}@teste.com",
            cargo = "Y",
            senha = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_funcionario_com_email_duplicado_deve_retornar_400()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var email = $"dup.{Guid.NewGuid():N}@teste.com";
        var request = new { nome = "X", email, cargo = "Y", senha = "SenhaForte123" };

        var primeiro = await client.PostAsJsonAsync("/api/funcionarios", request);
        Assert.Equal(HttpStatusCode.Created, primeiro.StatusCode);

        var segundo = await client.PostAsJsonAsync("/api/funcionarios", request);
        Assert.Equal(HttpStatusCode.BadRequest, segundo.StatusCode);
    }
}
