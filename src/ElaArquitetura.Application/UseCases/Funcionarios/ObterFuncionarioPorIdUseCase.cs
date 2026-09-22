using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed class ObterFuncionarioPorIdUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ObterFuncionarioPorIdUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<FuncionarioOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(id, cancellationToken);
        return funcionario is null ? null : FuncionarioOutput.DeFuncionario(funcionario);
    }
}
