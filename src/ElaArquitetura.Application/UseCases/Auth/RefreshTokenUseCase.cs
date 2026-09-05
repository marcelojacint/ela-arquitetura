using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record RefreshTokenInput(string RefreshToken);

public sealed record RefreshTokenOutput(string Token, string RefreshToken);

public sealed class RefreshTokenUseCase
{
    private static readonly TimeSpan RefreshTokenValidade = TimeSpan.FromDays(7);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IFuncionarioRepository funcionarioRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _funcionarioRepository = funcionarioRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<UseCaseResult<RefreshTokenOutput>> ExecutarAsync(RefreshTokenInput input, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenService.Hash(input.RefreshToken);
        var refreshToken = await _refreshTokenRepository.ObterPorHashAsync(hash, cancellationToken);

        if (refreshToken is null || !refreshToken.EstaValido())
            return UseCaseResult<RefreshTokenOutput>.Falha(new[] { "Refresh token inválido ou expirado." });

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(refreshToken.FuncionarioId, cancellationToken);
        if (funcionario is null || !funcionario.Ativo)
            return UseCaseResult<RefreshTokenOutput>.Falha(new[] { "Refresh token inválido ou expirado." });

        refreshToken.Revogar();
        await _refreshTokenRepository.AtualizarAsync(refreshToken, cancellationToken);

        var novoToken = _jwtTokenGenerator.GerarToken(funcionario);

        var novoRefreshTokenBruto = _refreshTokenService.GerarToken();
        var novoRefreshToken = new RefreshToken(
            funcionario.Id, _refreshTokenService.Hash(novoRefreshTokenBruto), DateTime.UtcNow.Add(RefreshTokenValidade));
        await _refreshTokenRepository.AdicionarAsync(novoRefreshToken, cancellationToken);

        return UseCaseResult<RefreshTokenOutput>.Ok(new RefreshTokenOutput(novoToken, novoRefreshTokenBruto));
    }
}
