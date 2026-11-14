using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record ParDeTokens(string Token, string RefreshToken);

/// <summary>Gera o JWT e um refresh token novo (guardando só o hash dele no banco).</summary>
public sealed class EmissorDeTokens
{
    private static readonly TimeSpan RefreshTokenValidade = TimeSpan.FromDays(7);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public EmissorDeTokens(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<ParDeTokens> EmitirAsync(Funcionario funcionario, CancellationToken cancellationToken)
    {
        var token = _jwtTokenGenerator.GerarToken(funcionario);

        var refreshTokenBruto = _refreshTokenService.GerarToken();
        var refreshToken = new RefreshToken(
            funcionario.Id, _refreshTokenService.Hash(refreshTokenBruto), DateTime.UtcNow.Add(RefreshTokenValidade));
        await _refreshTokenRepository.AdicionarAsync(refreshToken, cancellationToken);

        return new ParDeTokens(token, refreshTokenBruto);
    }
}
