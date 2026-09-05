using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record LoginInput(string Email, string Senha);

public sealed record LoginOutput(string Token, string RefreshToken, string Nome, string Cargo);

public sealed class LoginUseCase
{
    private static readonly TimeSpan RefreshTokenValidade = TimeSpan.FromDays(7);

    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginUseCase(
        IFuncionarioRepository funcionarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
    {
        _funcionarioRepository = funcionarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<UseCaseResult<LoginOutput>> ExecutarAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var funcionario = await _funcionarioRepository.ObterPorEmailAsync(input.Email, cancellationToken);

        if (funcionario is null || !funcionario.Ativo || !_passwordHasher.Verificar(input.Senha, funcionario.SenhaHash))
            return UseCaseResult<LoginOutput>.Falha(new[] { "Email ou senha inválidos." });

        var token = _jwtTokenGenerator.GerarToken(funcionario);

        var refreshTokenBruto = _refreshTokenService.GerarToken();
        var refreshToken = new RefreshToken(
            funcionario.Id, _refreshTokenService.Hash(refreshTokenBruto), DateTime.UtcNow.Add(RefreshTokenValidade));
        await _refreshTokenRepository.AdicionarAsync(refreshToken, cancellationToken);

        return UseCaseResult<LoginOutput>.Ok(new LoginOutput(token, refreshTokenBruto, funcionario.Nome, funcionario.Cargo));
    }
}
