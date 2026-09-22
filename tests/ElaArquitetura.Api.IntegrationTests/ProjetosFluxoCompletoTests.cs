using System.Net;
using System.Net.Http.Json;
using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Application.UseCases.Clientes;
using ElaArquitetura.Application.UseCases.Entregas;
using ElaArquitetura.Application.UseCases.Funcionarios;
using ElaArquitetura.Application.UseCases.Projetos;
using ElaArquitetura.Domain.Enums;
using Xunit;

namespace ElaArquitetura.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ProjetosFluxoCompletoTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ProjetosFluxoCompletoTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Projeto_percorre_as_6_etapas_ate_ser_concluido()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var cliente = await CriarClienteAsync(client);
        var projeto = await CriarProjetoAsync(client, cliente.Id);

        Assert.Equal(0, projeto.PercentualConcluido);
        Assert.Equal(StatusProjeto.EmAndamento, projeto.Status);

        var avancarSemChecklistResponse = await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
        Assert.Equal(HttpStatusCode.BadRequest, avancarSemChecklistResponse.StatusCode);

        var percentuaisEsperados = new[] { 20, 40, 60, 80, 100 };
        for (var i = 0; i < percentuaisEsperados.Length; i++)
        {
            await ConcluirChecklistDaEtapaAtualAsync(client, projeto.Id);

            var avancarResponse = await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
            Assert.Equal(HttpStatusCode.OK, avancarResponse.StatusCode);

            var projetoAtualizado = await avancarResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
            Assert.Equal(percentuaisEsperados[i], projetoAtualizado!.PercentualConcluido);

            if (i == 0)
            {
                var checklistEstudosPreliminares = await ObterChecklistAsync(client, projeto.Id);
                Assert.Equal(3, checklistEstudosPreliminares.Count);
                Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Briefing");
                Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Levantamento em Locação");
                Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Estudo de Layout");
                Assert.All(checklistEstudosPreliminares, item => Assert.False(item.Concluido));
            }
        }

        var concluirSemEntregaResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "Concluido" }));
        Assert.Equal(HttpStatusCode.BadRequest, concluirSemEntregaResponse.StatusCode);

        var entregaResponse = await client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/entrega", new
        {
            linkDrive = "https://drive.google.com/pasta-integracao",
            notificarWhatsApp = true
        });
        Assert.Equal(HttpStatusCode.OK, entregaResponse.StatusCode);
        var entrega = await entregaResponse.Content.ReadFromJsonAsync<EntregaOutput>(TestJson.Options);
        Assert.True(entrega!.EnviadoParaWhatsapp);

        var concluirResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "Concluido" }));
        Assert.Equal(HttpStatusCode.OK, concluirResponse.StatusCode);
        var projetoConcluido = await concluirResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal(StatusProjeto.Concluido, projetoConcluido!.Status);
        Assert.Equal(100, projetoConcluido.PercentualConcluido);

        var reabrirResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "EmAndamento" }));
        Assert.Equal(HttpStatusCode.OK, reabrirResponse.StatusCode);
        var projetoReaberto = await reabrirResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal(StatusProjeto.EmAndamento, projetoReaberto!.Status);
    }

    [Fact]
    public async Task Projeto_Executivo_recebe_as_7_sub_etapas_automaticamente()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await CriarProjetoAsync(client, cliente.Id);

        for (var i = 0; i < 3; i++)
        {
            await ConcluirChecklistDaEtapaAtualAsync(client, projeto.Id);
            await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
        }

        var checklistProjetoExecutivo = await ObterChecklistAsync(client, projeto.Id);
        Assert.Equal(7, checklistProjetoExecutivo.Count);
        Assert.Contains(checklistProjetoExecutivo, item => item.Descricao == "Render");
    }

    [Fact]
    public async Task Atribuir_e_remover_funcionario_do_projeto()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await CriarProjetoAsync(client, cliente.Id);

        var funcionarioResponse = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "Funcionario Projeto",
            email = $"proj.{Guid.NewGuid():N}@teste.com",
            cargo = "Arquiteta",
            senha = "SenhaForte123"
        });
        var funcionario = await funcionarioResponse.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options);

        var atribuirResponse = await client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/funcionarios", new
        {
            funcionarioId = funcionario!.Id,
            papelNoProjeto = "Responsavel"
        });
        Assert.Equal(HttpStatusCode.NoContent, atribuirResponse.StatusCode);

        var atribuirDuplicadoResponse = await client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/funcionarios", new
        {
            funcionarioId = funcionario.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, atribuirDuplicadoResponse.StatusCode);

        var removerResponse = await client.DeleteAsync($"/api/projetos/{projeto.Id}/funcionarios/{funcionario.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);
    }

    private static async Task<ClienteOutput> CriarClienteAsync(HttpClient client)
    {
        var telefone = $"119{Random.Shared.Next(10000000, 99999999)}";
        var response = await client.PostAsJsonAsync("/api/clientes", new { nome = "Cliente Fluxo", telefone });
        return (await response.Content.ReadFromJsonAsync<ClienteOutput>(TestJson.Options))!;
    }

    private static async Task<ProjetoOutput> CriarProjetoAsync(HttpClient client, Guid clienteId)
    {
        var response = await client.PostAsJsonAsync("/api/projetos", new { clienteId, titulo = "Projeto Fluxo Completo" });
        return (await response.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options))!;
    }

    private static async Task<List<ChecklistItemDetalheOutput>> ObterChecklistAsync(HttpClient client, Guid projetoId)
    {
        var response = await client.GetAsync($"/api/projetos/{projetoId}/checklist");
        return (await response.Content.ReadFromJsonAsync<List<ChecklistItemDetalheOutput>>(TestJson.Options))!;
    }

    private static async Task ConcluirChecklistDaEtapaAtualAsync(HttpClient client, Guid projetoId)
    {
        var itens = await ObterChecklistAsync(client, projetoId);

        if (itens.Count == 0)
        {
            var criarResponse = await client.PostAsJsonAsync($"/api/projetos/{projetoId}/checklist", new { descricao = "Item de teste" });
            var item = await criarResponse.Content.ReadFromJsonAsync<ChecklistItemCriadoOutput>(TestJson.Options);
            await client.PatchAsync($"/api/checklist/{item!.Id}/concluir", null);
            return;
        }

        foreach (var item in itens.Where(item => !item.Concluido))
            await client.PatchAsync($"/api/checklist/{item.Id}/concluir", null);
    }
}
