using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record LoginInput(string Email, string Senha);

public sealed record LoginOutput(string Token, string Nome, string Cargo);

/// <summary>
/// RF12 — autentica por email/senha. Mensagem de erro não diferencia
/// "email não existe" de "senha errada" para não vazar quais emails estão cadastrados.
/// </summary>
public sealed class LoginUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUseCase(
        IFuncionarioRepository funcionarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _funcionarioRepository = funcionarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UseCaseResult<LoginOutput>> ExecutarAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorEmailAsync(input.Email, cancellationToken);

        if (funcionario is null || !funcionario.Ativo || !_passwordHasher.Verificar(input.Senha, funcionario.SenhaHash))
            return UseCaseResult<LoginOutput>.Falha(new[] { "Email ou senha inválidos." });

        var token = _jwtTokenGenerator.GerarToken(funcionario);

        return UseCaseResult<LoginOutput>.Ok(new LoginOutput(token, funcionario.Nome, funcionario.Cargo));
    }
}
