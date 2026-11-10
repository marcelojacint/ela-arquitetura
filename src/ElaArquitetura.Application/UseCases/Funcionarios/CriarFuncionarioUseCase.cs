using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record CriarFuncionarioInput(string Nome, string Email, string? Telefone, DateOnly? DataNascimento);

/// <summary>Cadastra um funcionário do escritório, só para consulta (não ganha acesso ao sistema).</summary>
public sealed class CriarFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public CriarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository) => _funcionarioRepository = funcionarioRepository;

    public async Task<UseCaseResult<FuncionarioOutput>> ExecutarAsync(CriarFuncionarioInput input, CancellationToken cancellationToken)
    {
        var funcionario = Funcionario.Cadastrar(input.Nome, input.Email, input.Telefone, input.DataNascimento);
        if (!funcionario.IsValid)
            return UseCaseResult<FuncionarioOutput>.Falha(funcionario.Notifications.Select(n => n.Mensagem));

        var existente = await _funcionarioRepository.ObterPorEmailAsync(funcionario.Email, cancellationToken);
        if (existente is not null)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "Já existe um funcionário cadastrado com esse email." });

        await _funcionarioRepository.AdicionarAsync(funcionario, cancellationToken);

        return UseCaseResult<FuncionarioOutput>.Ok(FuncionarioOutput.DeFuncionario(funcionario));
    }
}
