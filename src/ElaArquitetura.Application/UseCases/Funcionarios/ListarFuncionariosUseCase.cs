using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

/// <summary>Funcionários do escritório exibidos no app: ativos e sem a usuária do sistema.</summary>
public sealed class ListarFuncionariosUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ListarFuncionariosUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<IReadOnlyCollection<FuncionarioOutput>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var funcionarios = await _funcionarioRepository.ListarCadastradosAsync(cancellationToken);
        return funcionarios.Select(FuncionarioOutput.DeFuncionario).ToList();
    }
}
