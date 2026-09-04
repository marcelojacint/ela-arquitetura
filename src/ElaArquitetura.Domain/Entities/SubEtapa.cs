namespace ElaArquitetura.Domain.Entities;

public class SubEtapa
{
    public Guid Id { get; private set; }
    public Guid EtapaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public int Ordem { get; private set; }

    protected SubEtapa()
    {
    }

    public SubEtapa(Guid id, Guid etapaId, string nome, int ordem)
    {
        Id = id;
        EtapaId = etapaId;
        Nome = nome;
        Ordem = ordem;
    }
}
