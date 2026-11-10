using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record AtualizarFuncionarioInput(Guid Id, string Nome, string Email, string? Telefone, DateOnly? DataNascimento);

public sealed class AtualizarFuncionarioUseCase
{
    public const string ErroNaoEncontrado = "Funcionário não encontrado.";

    private readonly IFuncionarioRepository _funcionarioRepository;

    public AtualizarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<UseCaseResult<FuncionarioOutput>> ExecutarAsync(AtualizarFuncionarioInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (funcionario is null || !funcionario.Ativo)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { ErroNaoEncontrado });

        // O e-mail da usuária do sistema é o login dela; não é alterado por esta tela.
        if (funcionario.PossuiAcesso)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "O cadastro da usuária do sistema não pode ser alterado por aqui." });

        funcionario.Atualizar(input.Nome, input.Email, input.Telefone, input.DataNascimento);
        if (!funcionario.IsValid)
            return UseCaseResult<FuncionarioOutput>.Falha(funcionario.Notifications.Select(n => n.Mensagem));

        var existenteComEmail = await _funcionarioRepository.ObterPorEmailAsync(funcionario.Email, cancellationToken);
        if (existenteComEmail is not null && existenteComEmail.Id != funcionario.Id)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "Já existe um funcionário cadastrado com esse email." });

        await _funcionarioRepository.AtualizarAsync(funcionario, cancellationToken);

        return UseCaseResult<FuncionarioOutput>.Ok(FuncionarioOutput.DeFuncionario(funcionario));
    }
}
