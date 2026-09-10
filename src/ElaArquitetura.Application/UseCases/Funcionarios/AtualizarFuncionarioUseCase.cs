using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record AtualizarFuncionarioInput(Guid Id, string Nome, string Email, string Cargo);

public sealed class AtualizarFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public AtualizarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<UseCaseResult<FuncionarioOutput>> ExecutarAsync(AtualizarFuncionarioInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (funcionario is null)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "Funcionário não encontrado." });

        var existenteComEmail = await _funcionarioRepository.ObterPorEmailAsync(input.Email, cancellationToken);
        if (existenteComEmail is not null && existenteComEmail.Id != funcionario.Id)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "Já existe um funcionário cadastrado com esse email." });

        funcionario.Atualizar(input.Nome, input.Email, input.Cargo);
        if (!funcionario.IsValid)
            return UseCaseResult<FuncionarioOutput>.Falha(funcionario.Notifications.Select(n => n.Mensagem));

        await _funcionarioRepository.AtualizarAsync(funcionario, cancellationToken);

        return UseCaseResult<FuncionarioOutput>.Ok(FuncionarioOutput.DeFuncionario(funcionario));
    }
}
