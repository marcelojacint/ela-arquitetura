namespace ElaArquitetura.Domain.Entities;

public class ProjetoFuncionario
{
    public Guid ProjetoId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public string? PapelNoProjeto { get; private set; }

    protected ProjetoFuncionario()
    {
    }

    public ProjetoFuncionario(Guid projetoId, Guid funcionarioId, string? papelNoProjeto = null)
    {
        ProjetoId = projetoId;
        FuncionarioId = funcionarioId;
        PapelNoProjeto = papelNoProjeto;
    }
}
