using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record DesativarFuncionarioInput(Guid Id);

public sealed class DesativarFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public DesativarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<UseCaseResult<bool>> ExecutarAsync(DesativarFuncionarioInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (funcionario is null)
            return UseCaseResult<bool>.Falha(new[] { "Funcionário não encontrado." });

        funcionario.Desativar();
        await _funcionarioRepository.AtualizarAsync(funcionario, cancellationToken);

        return UseCaseResult<bool>.Ok(true);
    }
}
