using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record LoginInput(string Email, string Senha);

public sealed record LoginOutput(string Token, string RefreshToken, string Nome, string? Cargo);

public sealed class LoginUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly EmissorDeTokens _emissorDeTokens;

    public LoginUseCase(
        IFuncionarioRepository funcionarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        EmissorDeTokens emissorDeTokens)
    {
        _funcionarioRepository = funcionarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _emissorDeTokens = emissorDeTokens;
    }

    public async Task<UseCaseResult<LoginOutput>> ExecutarAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorEmailAsync(input.Email, cancellationToken);

        // A senha é verificada mesmo quando o funcionário não existe, para o tempo de resposta
        // ser o mesmo em todos os casos e não revelar quais e-mails estão cadastrados.
        var senhaConfere = _passwordHasher.Verificar(input.Senha, funcionario?.SenhaHash);

        if (funcionario is null || !funcionario.Ativo || !funcionario.PossuiAcesso || !senhaConfere)
            return UseCaseResult<LoginOutput>.Falha(new[] { "Email ou senha inválidos." });

        await _refreshTokenRepository.RemoverExpiradosDoFuncionarioAsync(funcionario.Id, cancellationToken);
        var tokens = await _emissorDeTokens.EmitirAsync(funcionario, cancellationToken);

        return UseCaseResult<LoginOutput>.Ok(new LoginOutput(tokens.Token, tokens.RefreshToken, funcionario.Nome, funcionario.Cargo));
    }
}
