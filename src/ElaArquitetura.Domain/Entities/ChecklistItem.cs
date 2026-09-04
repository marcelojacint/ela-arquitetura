namespace ElaArquitetura.Domain.Entities;

public class ChecklistItem
{
    public Guid Id { get; private set; }
    public Guid ProjetoId { get; private set; }
    public Guid EtapaId { get; private set; }
    public Guid? SubEtapaId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public bool Concluido { get; private set; }
    public Guid? ConcluidoPor { get; private set; }
    public DateTime? DataConclusao { get; private set; }

    protected ChecklistItem()
    {
    }

    public ChecklistItem(Guid projetoId, Guid etapaId, string descricao, Guid? subEtapaId = null)
    {
        Id = Guid.NewGuid();
        ProjetoId = projetoId;
        EtapaId = etapaId;
        SubEtapaId = subEtapaId;
        Descricao = descricao;
        Concluido = false;
    }

    public void Concluir(Guid funcionarioId)
    {
        Concluido = true;
        ConcluidoPor = funcionarioId;
        DataConclusao = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        Concluido = false;
        ConcluidoPor = null;
        DataConclusao = null;
    }
}
