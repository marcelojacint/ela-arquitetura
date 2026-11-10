using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Clientes;
using ElaArquitetura.Application.UseCases.Funcionarios;
using ElaArquitetura.Application.UseCases.Projetos;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class FuncionariosTests
{
    private readonly CustomWebApplicationFactory _factory;

    public FuncionariosTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Fluxo_completo_criar_listar_obter_atualizar_excluir()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var email = $"func.{Guid.NewGuid():N}@teste.com";

        var criarResponse = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "Funcionaria Integracao",
            email,
            telefone = "(83) 99999-9999",
            dataNascimento = "2003-08-10"
        });
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var funcionario = await criarResponse.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);
        Assert.Equal("+5583999999999", funcionario!.Telefone);
        Assert.Equal(new DateOnly(2003, 8, 10), funcionario.DataNascimento);

        var listados = await client.GetFromJsonAsync<List<FuncionarioOutput>>("/api/funcionarios", TestJson.Options);
        Assert.Contains(listados!, f => f.Id == funcionario.Id);

        var obterResponse = await client.GetAsync($"/api/funcionarios/{funcionario.Id}");
        Assert.Equal(HttpStatusCode.OK, obterResponse.StatusCode);

        var atualizarResponse = await client.PutAsJsonAsync($"/api/funcionarios/{funcionario.Id}", new
        {
            nome = "Funcionaria Atualizada",
            email,
            telefone = (string?)null,
            dataNascimento = (string?)null
        });
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizado = await atualizarResponse.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);
        Assert.Equal("Funcionaria Atualizada", atualizado!.Nome);
        Assert.Null(atualizado.Telefone);
        Assert.Null(atualizado.DataNascimento);

        var excluirResponse = await client.DeleteAsync($"/api/funcionarios/{funcionario.Id}");
        Assert.Equal(HttpStatusCode.NoContent, excluirResponse.StatusCode);

        var listadosDepois = await client.GetFromJsonAsync<List<FuncionarioOutput>>("/api/funcionarios", TestJson.Options);
        Assert.DoesNotContain(listadosDepois!, f => f.Id == funcionario.Id);

        var obterDepois = await client.GetAsync($"/api/funcionarios/{funcionario.Id}");
        Assert.Equal(HttpStatusCode.NotFound, obterDepois.StatusCode);
    }

    [Fact]
    public async Task Email_de_funcionario_excluido_pode_ser_reutilizado()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var request = new { nome = "X", email = $"reuso.{Guid.NewGuid():N}@teste.com" };

        var primeiro = await client.PostAsJsonAsync("/api/funcionarios", request);
        var funcionario = await primeiro.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);
        await client.DeleteAsync($"/api/funcionarios/{funcionario!.Id}");

        var segundo = await client.PostAsJsonAsync("/api/funcionarios", request);
        Assert.Equal(HttpStatusCode.Created, segundo.StatusCode);
    }

    [Fact]
    public async Task Funcionario_excluido_nao_pode_ser_atribuido_a_projeto()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var clienteResponse = await client.PostAsJsonAsync("/api/clientes", new { nome = "Cliente Atribuicao", telefone = "83999990000" });
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ClienteOutput>(TestJson.Options);
        var projetoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente!.Id, titulo = "Projeto Atribuicao" });
        var projeto = await projetoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);

        var funcionarioResponse = await client.PostAsJsonAsync("/api/funcionarios", new { nome = "X", email = $"atrib.{Guid.NewGuid():N}@teste.com" });
        var funcionario = await funcionarioResponse.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);
        await client.DeleteAsync($"/api/funcionarios/{funcionario!.Id}");

        var atribuir = await client.PostAsJsonAsync($"/api/projetos/{projeto!.Id}/funcionarios", new { funcionarioId = funcionario.Id });

        Assert.Equal(HttpStatusCode.BadRequest, atribuir.StatusCode);
    }

    [Fact]
    public async Task Criar_funcionario_com_email_duplicado_deve_retornar_400()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var request = new { nome = "X", email = $"dup.{Guid.NewGuid():N}@teste.com" };

        var primeiro = await client.PostAsJsonAsync("/api/funcionarios", request);
        Assert.Equal(HttpStatusCode.Created, primeiro.StatusCode);

        var segundo = await client.PostAsJsonAsync("/api/funcionarios", request);
        Assert.Equal(HttpStatusCode.BadRequest, segundo.StatusCode);
    }

    [Fact]
    public async Task Criar_funcionario_com_nascimento_no_futuro_deve_retornar_400()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var response = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "X",
            email = $"{Guid.NewGuid():N}@teste.com",
            dataNascimento = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Usuaria_do_sistema_nao_aparece_na_lista_e_nao_pode_ser_excluida()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var listados = await client.GetFromJsonAsync<List<FuncionarioOutput>>("/api/funcionarios", TestJson.Options);
        Assert.DoesNotContain(listados!, f => f.Email == TestAuth.AdminEmail);

        var admin = await _factory.ObterAdminIdAsync();
        var excluirResponse = await client.DeleteAsync($"/api/funcionarios/{admin}");
        Assert.Equal(HttpStatusCode.BadRequest, excluirResponse.StatusCode);

        // O login continua funcionando depois da tentativa.
        await TestAuth.ComoAdminAsync(_factory.CreateClient());
    }

    [Fact]
    public async Task Funcionario_cadastrado_nao_consegue_fazer_login()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var email = $"semacesso.{Guid.NewGuid():N}@teste.com";
        await client.PostAsJsonAsync("/api/funcionarios", new { nome = "Sem Acesso", email });

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, senha = "qualquer-coisa" });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
