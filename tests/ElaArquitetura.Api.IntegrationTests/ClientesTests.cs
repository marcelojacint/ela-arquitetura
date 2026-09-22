using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Clientes;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ClientesTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ClientesTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Fluxo_completo_criar_buscar_atualizar_desativar()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var telefone = $"119{Random.Shared.Next(10000000, 99999999)}";

        var criarResponse = await client.PostAsJsonAsync("/api/clientes", new
        {
            nome = "Cliente Integracao",
            telefone,
            email = "cliente.integracao@teste.com",
            endereco = "Rua Teste, 1"
        });
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var cliente = await criarResponse.Content.ReadFromJsonAsync<ClienteOutput>(TestJson.Options);
        Assert.StartsWith("+55", cliente!.Telefone);

        var buscaResponse = await client.GetAsync($"/api/clientes?busca={telefone}");
        var busca = await buscaResponse.Content.ReadFromJsonAsync<List<ClienteOutput>>(TestJson.Options);
        Assert.Contains(busca!, c => c.Id == cliente.Id);

        var whatsappResponse = await client.GetAsync($"/api/clientes/{cliente.Id}/whatsapp-link");
        Assert.Equal(HttpStatusCode.OK, whatsappResponse.StatusCode);

        var atualizarResponse = await client.PutAsJsonAsync($"/api/clientes/{cliente.Id}", new
        {
            nome = "Cliente Integracao Atualizado",
            telefone,
            email = "cliente.integracao@teste.com",
            endereco = "Rua Nova, 2"
        });
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var desativarResponse = await client.DeleteAsync($"/api/clientes/{cliente.Id}");
        Assert.Equal(HttpStatusCode.NoContent, desativarResponse.StatusCode);

        var obterResponse = await client.GetAsync($"/api/clientes/{cliente.Id}");
        var clienteDesativado = await obterResponse.Content.ReadFromJsonAsync<ClienteOutput>(TestJson.Options);
        Assert.False(clienteDesativado!.Ativo);
    }

    [Fact]
    public async Task Criar_cliente_com_telefone_invalido_deve_retornar_400()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var response = await client.PostAsJsonAsync("/api/clientes", new { nome = "X", telefone = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Obter_cliente_inexistente_deve_retornar_404()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var response = await client.GetAsync($"/api/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
