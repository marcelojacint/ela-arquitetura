using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Domain.Entities;

public class Projeto : Notifiable
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? EnderecoObra { get; private set; }
    public string? LinkDrive { get; private set; }
    public StatusProjeto Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public Guid EtapaAtualId { get; private set; }

    protected Projeto()
    {
    }

    public static Projeto Criar(Guid clienteId, string titulo, Etapa etapaInicial, string? enderecoObra = null)
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Titulo = titulo?.Trim() ?? string.Empty,
            EnderecoObra = TextoOuNulo(enderecoObra),
            Status = StatusProjeto.EmAndamento,
            DataInicio = DateTime.UtcNow,
            EtapaAtualId = etapaInicial.Id
        };

        projeto.ValidarTitulo();

        return projeto;
    }

    /// <summary>Dados próprios do projeto: cada projeto do cliente tem o seu nome e o seu endereço de obra.</summary>
    public void AtualizarDados(string titulo, string? enderecoObra)
    {
        Titulo = titulo?.Trim() ?? string.Empty;
        EnderecoObra = TextoOuNulo(enderecoObra);
        ValidarTitulo();
    }

    /// <summary>Pasta do Drive com os arquivos do projeto. Vazio remove o link.</summary>
    public void DefinirLinkDrive(string? linkDrive)
    {
        var link = TextoOuNulo(linkDrive);

        if (link is not null && link.Length > 500)
        {
            AddNotification(nameof(LinkDrive), "Link do Drive deve ter no máximo 500 caracteres.");
            return;
        }

        if (link is not null
            && !(Uri.TryCreate(link, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
        {
            AddNotification(nameof(LinkDrive), "Informe um link válido, começando com https://.");
            return;
        }

        LinkDrive = link;
    }

    private static string? TextoOuNulo(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private void ValidarTitulo()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
            AddNotification(nameof(Titulo), "Título do projeto é obrigatório.");
        else if (Titulo.Length > 200)
            AddNotification(nameof(Titulo), "Título do projeto deve ter no máximo 200 caracteres.");
    }

    public bool PodeAvancarEtapa(Etapa etapaAtual, IEnumerable<ChecklistItem> checklistDaEtapaAtual)
    {
        if (etapaAtual.Opcional)
            return true;

        var itensDaEtapa = checklistDaEtapaAtual.Where(item => item.EtapaId == etapaAtual.Id).ToList();

        return itensDaEtapa.Count > 0 && itensDaEtapa.All(item => item.Concluido);
    }

    public void AvancarEtapa(Etapa etapaAtual, Etapa proximaEtapa, IEnumerable<ChecklistItem> checklistDaEtapaAtual)
    {
        if (!PodeAvancarEtapa(etapaAtual, checklistDaEtapaAtual))
        {
            AddNotification(nameof(EtapaAtualId), "Existem itens obrigatórios do checklist da etapa atual ainda não concluídos.");
            return;
        }

        EtapaAtualId = proximaEtapa.Id;
    }

    public void Concluir(Etapa etapaAtual, IReadOnlyCollection<Entrega> entregas)
    {
        if (etapaAtual.Id != EtapaAtualId)
        {
            AddNotification(nameof(Status), "Etapa informada não corresponde à etapa atual do projeto.");
            return;
        }

        if (!etapaAtual.Final)
        {
            AddNotification(nameof(Status), "O projeto só pode ser concluído após alcançar a etapa final do fluxo (Conclusão e Entrega).");
            return;
        }

        if (entregas.Count == 0)
        {
            AddNotification(nameof(Status), "É necessário registrar ao menos uma entrega antes de concluir o projeto.");
            return;
        }

        Status = StatusProjeto.Concluido;
        DataConclusao = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        Status = StatusProjeto.EmAndamento;
        DataConclusao = null;
    }
}
