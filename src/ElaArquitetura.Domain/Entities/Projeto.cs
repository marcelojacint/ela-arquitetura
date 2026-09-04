using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Domain.Entities;

public class Projeto : Notifiable
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public StatusProjeto Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public Guid EtapaAtualId { get; private set; }

    protected Projeto()
    {
    }

    public static Projeto Criar(Guid clienteId, string titulo, Etapa etapaInicial)
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Titulo = titulo,
            Status = StatusProjeto.EmAndamento,
            DataInicio = DateTime.UtcNow,
            EtapaAtualId = etapaInicial.Id
        };

        if (string.IsNullOrWhiteSpace(titulo))
            projeto.AddNotification(nameof(Titulo), "Título do projeto é obrigatório.");

        return projeto;
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
