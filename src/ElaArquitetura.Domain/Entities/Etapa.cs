namespace ElaArquitetura.Domain.Entities;

/// <summary>
/// As 6 etapas do fluxo são dado fixo (seed), não cadastro do usuário.
/// Opcional/Final controlam as regras de avanço e conclusão do Projeto.
/// </summary>
public class Etapa
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public int Ordem { get; private set; }
    public bool Opcional { get; private set; }
    public bool Final { get; private set; }

    protected Etapa()
    {
    }

    public Etapa(Guid id, string nome, int ordem, bool opcional = false, bool final = false)
    {
        Id = id;
        Nome = nome;
        Ordem = ordem;
        Opcional = opcional;
        Final = final;
    }
}
