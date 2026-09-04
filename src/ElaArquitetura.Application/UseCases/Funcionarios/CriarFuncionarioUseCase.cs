using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record CriarFuncionarioInput(string Nome, string Email, string Cargo, string Senha);

/// <summary>RF02/RNF10 — a senha em texto puro nunca chega ao Domain nem é persistida; só o hash.</summary>
public sealed class CriarFuncionarioUseCase
{
    private const int SenhaMinima = 8;

    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CriarFuncionarioUseCase(IFuncionarioRepository funcionarioRepository, IPasswordHasher passwordHasher)
    {
        _funcionarioRepository = funcionarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UseCaseResult<FuncionarioOutput>> ExecutarAsync(CriarFuncionarioInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Senha) || input.Senha.Length < SenhaMinima)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { $"Senha deve ter ao menos {SenhaMinima} caracteres." });

        var existente = await _funcionarioRepository.ObterPorEmailAsync(input.Email, cancellationToken);
        if (existente is not null)
            return UseCaseResult<FuncionarioOutput>.Falha(new[] { "Já existe um funcionário cadastrado com esse email." });

        var senhaHash = _passwordHasher.Hash(input.Senha);
        var funcionario = Funcionario.Criar(input.Nome, input.Email, input.Cargo, senhaHash);
        if (!funcionario.IsValid)
            return UseCaseResult<FuncionarioOutput>.Falha(funcionario.Notifications.Select(n => n.Mensagem));

        await _funcionarioRepository.AdicionarAsync(funcionario, cancellationToken);

        return UseCaseResult<FuncionarioOutput>.Ok(FuncionarioOutput.DeFuncionario(funcionario));
    }
}
