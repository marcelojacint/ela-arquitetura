using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record LogoutInput(string RefreshToken);

/// <summary>Revoga o refresh token da sessão que está saindo. Token desconhecido é ignorado (logout é idempotente).</summary>
public sealed class LogoutUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutUseCase(IRefreshTokenRepository refreshTokenRepository, IRefreshTokenService refreshTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
    }

    public async Task ExecutarAsync(LogoutInput input, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenService.Hash(input.RefreshToken);
        var refreshToken = await _refreshTokenRepository.ObterPorHashAsync(hash, cancellationToken);

        if (refreshToken is not null)
            await _refreshTokenRepository.TentarRevogarAsync(refreshToken.Id, cancellationToken);
    }
}
