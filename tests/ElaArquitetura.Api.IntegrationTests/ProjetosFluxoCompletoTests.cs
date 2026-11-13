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
    public async Task Projeto_comeca_em_estudos_preliminares_e_percorre_as_etapas_ate_ser_concluido()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());

        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);

        // "Cadastro do Cliente" é opcional e já está cumprida: o projeto nasce em Estudos Preliminares.
        Assert.Equal(20, projeto.PercentualConcluido);
        Assert.Equal(StatusProjeto.EmAndamento, projeto.Status);

        var checklistEstudosPreliminares = await ObterChecklistAsync(client, projeto.Id);
        Assert.Equal(3, checklistEstudosPreliminares.Count);
        Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Briefing");
        Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Levantamento em Locação");
        Assert.Contains(checklistEstudosPreliminares, item => item.Descricao == "Estudo de Layout");
        Assert.All(checklistEstudosPreliminares, item => Assert.False(item.Concluido));

        var avancarSemChecklistResponse = await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
        Assert.Equal(HttpStatusCode.BadRequest, avancarSemChecklistResponse.StatusCode);

        var percentuaisEsperados = new[] { 40, 60, 80, 100 };
        for (var i = 0; i < percentuaisEsperados.Length; i++)
        {
            await ConcluirChecklistDaEtapaAtualAsync(client, projeto.Id);

            var avancarResponse = await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
            Assert.Equal(HttpStatusCode.OK, avancarResponse.StatusCode);

            var projetoAtualizado = await avancarResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
            Assert.Equal(percentuaisEsperados[i], projetoAtualizado!.PercentualConcluido);

            if (i == 0)
            {
                var checklistAnteprojeto = await ObterChecklistAsync(client, projeto.Id);
                Assert.Single(checklistAnteprojeto);
                Assert.Equal("Maquete 3D", checklistAnteprojeto.First().Descricao);
            }
        }

        var concluirSemLinkResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "Concluido" }));
        Assert.Equal(HttpStatusCode.BadRequest, concluirSemLinkResponse.StatusCode);

        await client.PutAsJsonAsync($"/api/projetos/{projeto.Id}/link-drive", new { linkDrive = "https://drive.google.com/pasta-integracao" });

        var concluirResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "Concluido" }));
        Assert.Equal(HttpStatusCode.OK, concluirResponse.StatusCode);
        var projetoConcluido = await concluirResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal(StatusProjeto.Concluido, projetoConcluido!.Status);
        Assert.Equal(100, projetoConcluido.PercentualConcluido);

        // Com todos os projetos concluídos, o cliente sai de "Clientes" e vai para "Concluídos".
        var clientesEmAndamento = await client.GetFromJsonAsync<List<ClienteOutput>>("/api/clientes", TestJson.Options);
        Assert.DoesNotContain(clientesEmAndamento!, c => c.Id == cliente.Id);
        var concluidos = await client.GetFromJsonAsync<List<ClienteConcluidoOutput>>("/api/clientes/concluidos", TestJson.Options);
        var concluido = Assert.Single(concluidos!, c => c.Id == cliente.Id);
        Assert.Equal("https://drive.google.com/pasta-integracao", Assert.Single(concluido.Projetos).LinkDrive);

        // A entrega fica registrada quando o projeto é enviado ao cliente pelo WhatsApp.
        var entregaResponse = await client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/entrega", new
        {
            linkDrive = "https://drive.google.com/pasta-integracao",
            notificarWhatsApp = true
        });
        Assert.Equal(HttpStatusCode.OK, entregaResponse.StatusCode);
        var entrega = await entregaResponse.Content.ReadFromJsonAsync<EntregaOutput>(TestJson.Options);
        Assert.True(entrega!.EnviadoParaWhatsapp);

        var reabrirResponse = await client.PatchAsync(
            $"/api/projetos/{projeto.Id}/status", JsonContent.Create(new { status = "EmAndamento" }));
        Assert.Equal(HttpStatusCode.OK, reabrirResponse.StatusCode);
        var projetoReaberto = await reabrirResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal(StatusProjeto.EmAndamento, projetoReaberto!.Status);

        var clientesDepoisDeReabrir = await client.GetFromJsonAsync<List<ClienteOutput>>("/api/clientes", TestJson.Options);
        Assert.Contains(clientesDepoisDeReabrir!, c => c.Id == cliente.Id);
    }

    [Fact]
    public async Task Projeto_Executivo_recebe_as_5_sub_etapas_automaticamente()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);

        for (var i = 0; i < 2; i++)
        {
            await ConcluirChecklistDaEtapaAtualAsync(client, projeto.Id);
            await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
        }

        var checklistProjetoExecutivo = await ObterChecklistAsync(client, projeto.Id);
        Assert.Equal(5, checklistProjetoExecutivo.Count);
        Assert.Contains(checklistProjetoExecutivo, item => item.Descricao == "Detalhamento de Pedras e Bancadas");
    }

    [Fact]
    public async Task Cadastro_do_cliente_ja_abre_o_projeto_com_o_checklist_de_todas_as_etapas()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);

        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);
        Assert.Equal(StatusProjeto.EmAndamento, projeto.Status);

        var etapas = await ObterChecklistCompletaAsync(client, projeto.Id);
        Assert.True(etapas.Single(e => e.Nome == "Estudos Preliminares").Atual);
        Assert.Equal(
            new[] { "Briefing", "Levantamento em Locação", "Estudo de Layout" },
            etapas.Single(e => e.Nome == "Estudos Preliminares").Itens.Select(i => i.Descricao));
        Assert.Equal(new[] { "Maquete 3D" }, etapas.Single(e => e.Nome == "Anteprojeto").Itens.Select(i => i.Descricao));
        Assert.Equal(5, etapas.Single(e => e.Nome == "Projeto Executivo").Itens.Count);
        Assert.Equal(new[] { "Relatório de Obra" }, etapas.Single(e => e.Nome == "Acompanhamentos").Itens.Select(i => i.Descricao));

        // Toda sub-etapa vira exatamente um item.
        var todasSubEtapas = etapas.SelectMany(e => e.SubEtapas).Select(s => s.Id).OrderBy(id => id);
        var subEtapasDosItens = etapas.SelectMany(e => e.Itens).Select(i => i.SubEtapaId!.Value).OrderBy(id => id);
        Assert.Equal(todasSubEtapas, subEtapasDosItens);
    }

    [Fact]
    public async Task Item_personalizado_pode_ser_adicionado_e_removido_em_etapa_futura()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);
        var projetoExecutivo = (await ObterChecklistCompletaAsync(client, projeto.Id)).Single(e => e.Nome == "Projeto Executivo");

        var criarResponse = await client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/checklist", new
        {
            descricao = "Detalhamento de Iluminação",
            etapaId = projetoExecutivo.EtapaId
        });
        Assert.Equal(HttpStatusCode.OK, criarResponse.StatusCode);
        var item = await criarResponse.Content.ReadFromJsonAsync<ChecklistItemCriadoOutput>(TestJson.Options);
        Assert.Equal(projetoExecutivo.EtapaId, item!.EtapaId);
        Assert.Null(item.SubEtapaId);

        var etapaDepois = (await ObterChecklistCompletaAsync(client, projeto.Id)).Single(e => e.Nome == "Projeto Executivo");
        Assert.Equal(6, etapaDepois.Itens.Count);
        Assert.Equal("Detalhamento de Iluminação", etapaDepois.Itens.Last().Descricao);

        var removerResponse = await client.DeleteAsync($"/api/checklist/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, removerResponse.StatusCode);
    }

    [Fact]
    public async Task Avancar_etapa_pula_as_etapas_seguintes_que_ja_estao_com_checklist_concluido()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);
        var etapas = await ObterChecklistCompletaAsync(client, projeto.Id);

        // Anteprojeto concluído antes da hora; depois termina Estudos Preliminares.
        foreach (var item in etapas.Single(e => e.Nome == "Anteprojeto").Itens)
            await client.PatchAsync($"/api/checklist/{item.Id}/concluir", null);
        await ConcluirChecklistDaEtapaAtualAsync(client, projeto.Id);

        var avancarResponse = await client.PatchAsync($"/api/projetos/{projeto.Id}/avancar-etapa", null);
        Assert.Equal(HttpStatusCode.OK, avancarResponse.StatusCode);
        var projetoAtualizado = await avancarResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);

        Assert.Equal(etapas.Single(e => e.Nome == "Projeto Executivo").EtapaId, projetoAtualizado!.EtapaAtualId);
        Assert.Equal(60, projetoAtualizado.PercentualConcluido);
    }

    [Fact]
    public async Task Cliente_pode_ter_um_segundo_projeto_com_checklist_proprio_e_nome_unico()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var primeiro = await ObterProjetoDoClienteAsync(client, cliente.Id);
        Assert.Equal(cliente.Nome, primeiro.Titulo);

        var segundoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "Casa de Praia" });
        Assert.Equal(HttpStatusCode.Created, segundoResponse.StatusCode);
        var segundo = await segundoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);

        // Nome repetido no mesmo cliente é recusado (sem diferenciar maiúsculas).
        var repetidoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "casa de praia" });
        Assert.Equal(HttpStatusCode.BadRequest, repetidoResponse.StatusCode);

        // Cada projeto tem o próprio checklist: marcar no segundo não mexe no primeiro.
        var itemDoSegundo = (await ObterChecklistAsync(client, segundo!.Id)).First();
        await client.PatchAsync($"/api/checklist/{itemDoSegundo.Id}/concluir", null);
        Assert.All(await ObterChecklistAsync(client, primeiro.Id), item => Assert.False(item.Concluido));

        var projetos = await client.GetFromJsonAsync<List<ProjetoOutput>>($"/api/projetos?clienteId={cliente.Id}", TestJson.Options);
        Assert.Equal(2, projetos!.Count);

        var clientes = await client.GetFromJsonAsync<List<ClienteOutput>>("/api/clientes", TestJson.Options);
        Assert.Equal(2, clientes!.Single(c => c.Id == cliente.Id).QuantidadeProjetos);
    }

    [Fact]
    public async Task Editar_projeto_troca_nome_endereco_e_responsavel_e_recusa_nome_vazio_ou_repetido()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var primeiro = await ObterProjetoDoClienteAsync(client, cliente.Id);
        await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "Casa de Praia" });
        var funcionaria = await CriarFuncionarioAsync(client);

        var editarResponse = await client.PutAsJsonAsync($"/api/projetos/{primeiro.Id}", new
        {
            titulo = "Apartamento Centro",
            enderecoObra = "Rua das Flores, 10",
            funcionarioResponsavelId = funcionaria.Id
        });
        Assert.Equal(HttpStatusCode.OK, editarResponse.StatusCode);
        var editado = await editarResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal("Apartamento Centro", editado!.Titulo);
        Assert.Equal("Rua das Flores, 10", editado.EnderecoObra);
        Assert.Equal(funcionaria.Id, Assert.Single(editado.Funcionarios).FuncionarioId);

        // Sem responsável e sem endereço: o projeto fica sem os dois. Manter o próprio nome não conta como repetido.
        var limparResponse = await client.PutAsJsonAsync($"/api/projetos/{primeiro.Id}", new
        {
            titulo = "Apartamento Centro",
            enderecoObra = (string?)null,
            funcionarioResponsavelId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.OK, limparResponse.StatusCode);
        var limpo = await limparResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Null(limpo!.EnderecoObra);
        Assert.Empty(limpo.Funcionarios);

        var repetidoResponse = await client.PutAsJsonAsync($"/api/projetos/{primeiro.Id}", new { titulo = "Casa de Praia" });
        Assert.Equal(HttpStatusCode.BadRequest, repetidoResponse.StatusCode);

        var vazioResponse = await client.PutAsJsonAsync($"/api/projetos/{primeiro.Id}", new { titulo = " " });
        Assert.Equal(HttpStatusCode.BadRequest, vazioResponse.StatusCode);

        var responsavelInexistenteResponse = await client.PutAsJsonAsync($"/api/projetos/{primeiro.Id}", new
        {
            titulo = "Apartamento Centro",
            funcionarioResponsavelId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.BadRequest, responsavelInexistenteResponse.StatusCode);
    }

    [Fact]
    public async Task Endereco_da_obra_e_de_cada_projeto_e_nao_passa_para_o_segundo()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var telefone = $"119{Random.Shared.Next(10000000, 99999999)}";
        var clienteResponse = await client.PostAsJsonAsync("/api/clientes", new
        {
            nome = "Cliente Endereco",
            telefone,
            enderecoObra = "Rua da Primeira Obra, 1"
        });
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ClienteOutput>(TestJson.Options);

        var primeiro = await ObterProjetoDoClienteAsync(client, cliente!.Id);
        Assert.Equal("Rua da Primeira Obra, 1", primeiro.EnderecoObra);

        var funcionaria = await CriarFuncionarioAsync(client);
        var segundoResponse = await client.PostAsJsonAsync("/api/projetos", new
        {
            clienteId = cliente.Id,
            titulo = "Casa de Campo",
            enderecoObra = "Estrada do Sítio, km 3",
            funcionarioResponsavelId = funcionaria.Id
        });
        var segundo = await segundoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Equal("Estrada do Sítio, km 3", segundo!.EnderecoObra);

        var semEnderecoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "Escritório" });
        var semEndereco = await semEnderecoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        Assert.Null(semEndereco!.EnderecoObra);

        var segundoConsultado = await client.GetFromJsonAsync<ProjetoOutput>($"/api/projetos/{segundo.Id}", TestJson.Options);
        Assert.Equal(funcionaria.Id, Assert.Single(segundoConsultado!.Funcionarios).FuncionarioId);
    }

    [Fact]
    public async Task Excluir_projeto_remove_ele_e_o_checklist_mas_nao_deixa_excluir_o_unico_projeto()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var primeiro = await ObterProjetoDoClienteAsync(client, cliente.Id);

        var unicoResponse = await client.DeleteAsync($"/api/projetos/{primeiro.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, unicoResponse.StatusCode);

        var segundoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "Casa de Praia" });
        var segundo = await segundoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options);
        var itemDoSegundo = (await ObterChecklistAsync(client, segundo!.Id)).First();
        await client.PatchAsync($"/api/checklist/{itemDoSegundo.Id}/concluir", null);

        var excluirResponse = await client.DeleteAsync($"/api/projetos/{segundo.Id}");
        Assert.Equal(HttpStatusCode.NoContent, excluirResponse.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/projetos/{segundo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/projetos/{segundo.Id}")).StatusCode);
        var restante = await ObterProjetoDoClienteAsync(client, cliente.Id);
        Assert.Equal(primeiro.Id, restante.Id);

        var clientes = await client.GetFromJsonAsync<List<ClienteOutput>>("/api/clientes", TestJson.Options);
        Assert.Equal(1, clientes!.Single(c => c.Id == cliente.Id).QuantidadeProjetos);
    }

    [Fact]
    public async Task Cliente_com_dois_projetos_so_vai_para_concluidos_quando_os_dois_forem_concluidos_e_volta_ao_reabrir_um()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var primeiro = await ObterProjetoDoClienteAsync(client, cliente.Id);
        var segundoResponse = await client.PostAsJsonAsync("/api/projetos", new { clienteId = cliente.Id, titulo = "Casa de Praia" });
        var segundo = (await segundoResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options))!;

        await ConcluirProjetoAsync(client, primeiro.Id);
        Assert.Contains(await ListarClientesAsync(client), c => c.Id == cliente.Id);
        Assert.DoesNotContain(await ListarConcluidosAsync(client), c => c.Id == cliente.Id);

        await ConcluirProjetoAsync(client, segundo.Id);
        Assert.DoesNotContain(await ListarClientesAsync(client), c => c.Id == cliente.Id);
        var concluido = Assert.Single(await ListarConcluidosAsync(client), c => c.Id == cliente.Id);
        Assert.Equal(2, concluido.Projetos.Count);

        var reabrirResponse = await client.PatchAsync(
            $"/api/projetos/{segundo.Id}/status", JsonContent.Create(new { status = "EmAndamento" }));
        Assert.Equal(HttpStatusCode.OK, reabrirResponse.StatusCode);
        Assert.Contains(await ListarClientesAsync(client), c => c.Id == cliente.Id);
        Assert.DoesNotContain(await ListarConcluidosAsync(client), c => c.Id == cliente.Id);
    }

    private static async Task<List<ClienteOutput>> ListarClientesAsync(HttpClient client)
        => (await client.GetFromJsonAsync<List<ClienteOutput>>("/api/clientes", TestJson.Options))!;

    private static async Task<List<ClienteConcluidoOutput>> ListarConcluidosAsync(HttpClient client)
        => (await client.GetFromJsonAsync<List<ClienteConcluidoOutput>>("/api/clientes/concluidos", TestJson.Options))!;

    /// <summary>Marca o checklist de todas as etapas, define o link do Drive e conclui.</summary>
    private static async Task ConcluirProjetoAsync(HttpClient client, Guid projetoId)
    {
        for (var i = 0; i < 4; i++)
        {
            await ConcluirChecklistDaEtapaAtualAsync(client, projetoId);
            await client.PatchAsync($"/api/projetos/{projetoId}/avancar-etapa", null);
        }

        await client.PutAsJsonAsync($"/api/projetos/{projetoId}/link-drive", new { linkDrive = "https://drive.google.com/pasta" });
        var response = await client.PatchAsync($"/api/projetos/{projetoId}/status", JsonContent.Create(new { status = "Concluido" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Link_do_drive_pode_ser_definido_trocado_e_removido()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);
        Assert.Null(projeto.LinkDrive);

        var definirResponse = await client.PutAsJsonAsync($"/api/projetos/{projeto.Id}/link-drive", new { linkDrive = "https://drive.google.com/pasta-1" });
        Assert.Equal(HttpStatusCode.OK, definirResponse.StatusCode);
        Assert.Equal("https://drive.google.com/pasta-1", (await definirResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options))!.LinkDrive);

        await client.PutAsJsonAsync($"/api/projetos/{projeto.Id}/link-drive", new { linkDrive = "https://drive.google.com/pasta-2" });
        var consultado = await client.GetFromJsonAsync<ProjetoOutput>($"/api/projetos/{projeto.Id}", TestJson.Options);
        Assert.Equal("https://drive.google.com/pasta-2", consultado!.LinkDrive);

        var invalidoResponse = await client.PutAsJsonAsync($"/api/projetos/{projeto.Id}/link-drive", new { linkDrive = "pasta sem link" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidoResponse.StatusCode);

        var vazioResponse = await client.PutAsJsonAsync($"/api/projetos/{projeto.Id}/link-drive", new { linkDrive = " " });
        Assert.Equal(HttpStatusCode.BadRequest, vazioResponse.StatusCode);

        var removerResponse = await client.DeleteAsync($"/api/projetos/{projeto.Id}/link-drive");
        Assert.Equal(HttpStatusCode.OK, removerResponse.StatusCode);
        Assert.Null((await removerResponse.Content.ReadFromJsonAsync<ProjetoOutput>(TestJson.Options))!.LinkDrive);
    }

    [Fact]
    public async Task Atribuir_e_remover_funcionario_do_projeto()
    {
        var client = await TestAuth.ComoAdminAsync(_factory.CreateClient());
        var cliente = await CriarClienteAsync(client);
        var projeto = await ObterProjetoDoClienteAsync(client, cliente.Id);

        var funcionarioResponse = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "Funcionario Projeto",
            email = $"proj.{Guid.NewGuid():N}@teste.com"
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

    private static async Task<FuncionarioOutput> CriarFuncionarioAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/funcionarios", new
        {
            nome = "Funcionaria Responsavel",
            email = $"resp.{Guid.NewGuid():N}@teste.com"
        });
        return (await response.Content.ReadFromJsonAsync<FuncionarioOutput>(TestJson.Options))!;
    }

    /// <summary>O projeto é aberto junto com o cadastro do cliente.</summary>
    private static async Task<ProjetoOutput> ObterProjetoDoClienteAsync(HttpClient client, Guid clienteId)
    {
        var projetos = await client.GetFromJsonAsync<List<ProjetoOutput>>($"/api/projetos?clienteId={clienteId}", TestJson.Options);
        return Assert.Single(projetos!);
    }

    private static async Task<List<EtapaDoChecklistOutput>> ObterChecklistCompletaAsync(HttpClient client, Guid projetoId)
        => (await client.GetFromJsonAsync<List<EtapaDoChecklistOutput>>($"/api/projetos/{projetoId}/checklist/completa", TestJson.Options))!;

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
