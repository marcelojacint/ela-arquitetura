namespace ElaArquitetura.Application.Common;

public sealed class UseCaseResult<T>
{
    public bool Sucesso { get; }
    public T? Dados { get; }
    public IReadOnlyCollection<string> Erros { get; }

    private UseCaseResult(bool sucesso, T? dados, IReadOnlyCollection<string> erros)
    {
        Sucesso = sucesso;
        Dados = dados;
        Erros = erros;
    }

    public static UseCaseResult<T> Ok(T dados) => new(true, dados, Array.Empty<string>());

    public static UseCaseResult<T> Falha(IEnumerable<string> erros) => new(false, default, erros.ToList());
}
