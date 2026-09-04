using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed class ListarFuncionariosUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ListarFuncionariosUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<IReadOnlyCollection<FuncionarioOutput>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var funcionarios = await _funcionarioRepository.ListarAsync(cancellationToken);
        return funcionarios.Select(FuncionarioOutput.DeFuncionario).ToList();
    }
}
