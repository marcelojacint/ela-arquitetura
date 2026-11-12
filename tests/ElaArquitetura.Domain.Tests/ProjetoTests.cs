using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;
using Xunit;

namespace ElaArquitetura.Domain.Tests;

public class ProjetoTests
{
    private static Etapa NovaEtapa(string nome, int ordem, bool opcional = false, bool final = false)
        => new(Guid.NewGuid(), nome, ordem, opcional, final);

    [Fact]
    public void Criar_deve_guardar_endereco_da_obra_sem_espacos_extras()
    {
        var projeto = Projeto.Criar(Guid.NewGuid(), "Maria Silva", NovaEtapa("Estudos Preliminares", 2), "  Rua da Obra, 50  ");

        Assert.Equal("Rua da Obra, 50", projeto.EnderecoObra);
    }

    [Fact]
    public void AtualizarDados_deve_trocar_titulo_e_endereco_e_deixar_endereco_vazio_como_nulo()
    {
        var projeto = Projeto.Criar(Guid.NewGuid(), "Maria Silva", NovaEtapa("Estudos Preliminares", 2), "Rua A, 1");

        projeto.AtualizarDados("  Casa de Praia  ", "   ");

        Assert.True(projeto.IsValid);
        Assert.Equal("Casa de Praia", projeto.Titulo);
        Assert.Null(projeto.EnderecoObra);
    }

    [Theory]
    [InlineData("  https://drive.google.com/drive/folders/abc  ", "https://drive.google.com/drive/folders/abc")]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void DefinirLinkDrive_deve_guardar_link_sem_espacos_ou_remover_quando_vazio(string? entrada, string? esperado)
    {
        var projeto = Projeto.Criar(Guid.NewGuid(), "Maria Silva", NovaEtapa("Estudos Preliminares", 2));
        projeto.DefinirLinkDrive("https://drive.google.com/antigo");

        projeto.DefinirLinkDrive(entrada);

        Assert.True(projeto.IsValid);
        Assert.Equal(esperado, projeto.LinkDrive);
    }

    [Theory]
    [InlineData("drive.google.com/pasta")]
    [InlineData("ftp://drive.google.com/pasta")]
    [InlineData("não é link")]
    public void DefinirLinkDrive_deve_recusar_link_invalido_e_manter_o_anterior(string entrada)
    {
        var projeto = Projeto.Criar(Guid.NewGuid(), "Maria Silva", NovaEtapa("Estudos Preliminares", 2));
        projeto.DefinirLinkDrive("https://drive.google.com/antigo");

        projeto.DefinirLinkDrive(entrada);

        Assert.False(projeto.IsValid);
        Assert.Equal("https://drive.google.com/antigo", projeto.LinkDrive);
    }

    [Fact]
    public void AtualizarDados_deve_notificar_quando_titulo_esta_vazio()
    {
        var projeto = Projeto.Criar(Guid.NewGuid(), "Maria Silva", NovaEtapa("Estudos Preliminares", 2));

        projeto.AtualizarDados("   ", null);

        Assert.False(projeto.IsValid);
    }

    [Fact]
    public void PodeAvancarEtapa_deve_ser_falso_quando_checklist_obrigatorio_esta_incompleto()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);
        var checklist = new[] { new ChecklistItem(projeto.Id, etapaAtual.Id, "Aprovação do cliente") };

        Assert.False(projeto.PodeAvancarEtapa(etapaAtual, checklist));
    }

    [Fact]
    public void PodeAvancarEtapa_deve_ser_verdadeiro_quando_todo_checklist_obrigatorio_esta_concluido()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);
        var item = new ChecklistItem(projeto.Id, etapaAtual.Id, "Aprovação do cliente");
        item.Concluir(Guid.NewGuid());

        Assert.True(projeto.PodeAvancarEtapa(etapaAtual, new[] { item }));
    }

    [Fact]
    public void PodeAvancarEtapa_deve_ser_verdadeiro_para_etapa_opcional_mesmo_com_checklist_vazio()
    {
        var etapaAtual = NovaEtapa("Relatório de Obra", 5, opcional: true);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);

        Assert.True(projeto.PodeAvancarEtapa(etapaAtual, Array.Empty<ChecklistItem>()));
    }

    [Fact]
    public void PodeAvancarEtapa_deve_ser_falso_para_etapa_obrigatoria_com_checklist_vazio()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);

        Assert.False(projeto.PodeAvancarEtapa(etapaAtual, Array.Empty<ChecklistItem>()));
    }

    [Fact]
    public void AvancarEtapa_nao_deve_mudar_etapa_quando_checklist_esta_incompleto()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var proximaEtapa = NovaEtapa("Projeto Executivo", 4);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);
        var checklist = new[] { new ChecklistItem(projeto.Id, etapaAtual.Id, "Aprovação do cliente") };

        projeto.AvancarEtapa(etapaAtual, proximaEtapa, checklist);

        Assert.False(projeto.IsValid);
        Assert.Equal(etapaAtual.Id, projeto.EtapaAtualId);
    }

    [Fact]
    public void AvancarEtapa_deve_mudar_etapa_quando_checklist_esta_completo()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var proximaEtapa = NovaEtapa("Projeto Executivo", 4);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);
        var item = new ChecklistItem(projeto.Id, etapaAtual.Id, "Aprovação do cliente");
        item.Concluir(Guid.NewGuid());

        projeto.AvancarEtapa(etapaAtual, proximaEtapa, new[] { item });

        Assert.True(projeto.IsValid);
        Assert.Equal(proximaEtapa.Id, projeto.EtapaAtualId);
    }

    [Fact]
    public void Concluir_deve_gerar_notification_quando_nao_ha_entrega_registrada()
    {
        var etapaFinal = NovaEtapa("Conclusão e Entrega", 6, final: true);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaFinal);

        projeto.Concluir(etapaFinal, Array.Empty<Entrega>());

        Assert.False(projeto.IsValid);
        Assert.Equal(StatusProjeto.EmAndamento, projeto.Status);
    }

    [Fact]
    public void Concluir_deve_gerar_notification_quando_etapa_atual_nao_e_final()
    {
        var etapaAtual = NovaEtapa("Anteprojeto", 3);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaAtual);
        var entrega = new Entrega(projeto.Id, "https://drive.google.com/xyz");

        projeto.Concluir(etapaAtual, new[] { entrega });

        Assert.False(projeto.IsValid);
        Assert.Equal(StatusProjeto.EmAndamento, projeto.Status);
    }

    [Fact]
    public void Concluir_deve_mudar_status_quando_etapa_e_final_e_ha_entrega_registrada()
    {
        var etapaFinal = NovaEtapa("Conclusão e Entrega", 6, final: true);
        var projeto = Projeto.Criar(Guid.NewGuid(), "Casa Silva", etapaFinal);
        var entrega = new Entrega(projeto.Id, "https://drive.google.com/xyz");

        projeto.Concluir(etapaFinal, new[] { entrega });

        Assert.True(projeto.IsValid);
        Assert.Equal(StatusProjeto.Concluido, projeto.Status);
        Assert.NotNull(projeto.DataConclusao);
    }
}
