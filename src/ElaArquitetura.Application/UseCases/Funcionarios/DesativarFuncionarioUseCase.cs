using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record DesativarFuncionarioInput(Guid Id);

/// <summary>
/// "Excluir" no app. O registro é só desativado, para não quebrar o histórico dos projetos
/// em que o funcionário foi atribuído; ele some da listagem e o e-mail fica livre para reuso.
/// </summary>
public sealed class DesativarFuncionarioUseCase
{
    public const string ErroNaoEncontrado = "Funcionário não encontrado.";

    private readonly IFuncionarioRepository _funcionarioRepository;

    public DesativarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<UseCaseResult<bool>> ExecutarAsync(DesativarFuncionarioInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (funcionario is null || !funcionario.Ativo)
            return UseCaseResult<bool>.Falha(new[] { ErroNaoEncontrado });

        // Desativar a usuária do sistema bloquearia o único login existente.
        if (funcionario.PossuiAcesso)
            return UseCaseResult<bool>.Falha(new[] { "A usuária do sistema não pode ser excluída." });

        funcionario.Desativar();
        await _funcionarioRepository.AtualizarAsync(funcionario, cancellationToken);

        return UseCaseResult<bool>.Ok(true);
    }
}
