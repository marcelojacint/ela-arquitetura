using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Auth;

public sealed record RefreshTokenInput(string RefreshToken);

public sealed record RefreshTokenOutput(string Token, string RefreshToken);

public sealed class RefreshTokenUseCase
{
    private const string ErroTokenInvalido = "Refresh token inválido ou expirado.";

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly EmissorDeTokens _emissorDeTokens;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IFuncionarioRepository funcionarioRepository,
        IRefreshTokenService refreshTokenService,
        EmissorDeTokens emissorDeTokens)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _funcionarioRepository = funcionarioRepository;
        _refreshTokenService = refreshTokenService;
        _emissorDeTokens = emissorDeTokens;
    }

    public async Task<UseCaseResult<RefreshTokenOutput>> ExecutarAsync(RefreshTokenInput input, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenService.Hash(input.RefreshToken);
        var refreshToken = await _refreshTokenRepository.ObterPorHashAsync(hash, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiraEm <= DateTime.UtcNow)
            return Falha();

        // A revogação é atômica no banco. Se não deu para revogar, o token já tinha sido usado
        // (por outra requisição ou por quem o roubou): por segurança, encerra todas as sessões do funcionário.
        if (!await _refreshTokenRepository.TentarRevogarAsync(refreshToken.Id, cancellationToken))
        {
            await _refreshTokenRepository.RevogarTodosDoFuncionarioAsync(refreshToken.FuncionarioId, cancellationToken);
            return Falha();
        }

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(refreshToken.FuncionarioId, cancellationToken);
        if (funcionario is null || !funcionario.Ativo || !funcionario.PossuiAcesso)
        {
            await _refreshTokenRepository.RevogarTodosDoFuncionarioAsync(refreshToken.FuncionarioId, cancellationToken);
            return Falha();
        }

        var tokens = await _emissorDeTokens.EmitirAsync(funcionario, cancellationToken);

        return UseCaseResult<RefreshTokenOutput>.Ok(new RefreshTokenOutput(tokens.Token, tokens.RefreshToken));
    }

    private static UseCaseResult<RefreshTokenOutput> Falha() => UseCaseResult<RefreshTokenOutput>.Falha(new[] { ErroTokenInvalido });
}
