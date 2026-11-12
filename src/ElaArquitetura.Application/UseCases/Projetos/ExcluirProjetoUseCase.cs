using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ExcluirProjetoInput(Guid ProjetoId);

public enum ResultadoExclusaoProjeto
{
    Excluido,
    NaoEncontrado,
    UltimoProjetoDoCliente
}

/// <summary>
/// Exclui o projeto de vez, com o checklist, as entregas e os vínculos com funcionários.
/// Todo cliente precisa ter um projeto, então o último projeto do cliente não pode ser excluído
/// (para tirar o cliente de vez, exclui-se o cliente).
/// </summary>
public sealed class ExcluirProjetoUseCase
{
    public const string MensagemUltimoProjeto = "Não é possível excluir o único projeto do cliente.";

    private readonly IProjetoRepository _projetoRepository;

    public ExcluirProjetoUseCase(IProjetoRepository projetoRepository) => _projetoRepository = projetoRepository;

    public async Task<ResultadoExclusaoProjeto> ExecutarAsync(ExcluirProjetoInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return ResultadoExclusaoProjeto.NaoEncontrado;

        var projetosDoCliente = await _projetoRepository.ContarPorClientesAsync(new[] { projeto.ClienteId }, cancellationToken);
        if (projetosDoCliente.GetValueOrDefault(projeto.ClienteId) <= 1)
            return ResultadoExclusaoProjeto.UltimoProjetoDoCliente;

        await _projetoRepository.RemoverAsync(projeto, cancellationToken);
        return ResultadoExclusaoProjeto.Excluido;
    }
}
