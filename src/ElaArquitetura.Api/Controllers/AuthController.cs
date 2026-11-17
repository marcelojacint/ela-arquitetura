using ElaArquitetura.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly LogoutUseCase _logoutUseCase;

    public AuthController(LoginUseCase loginUseCase, RefreshTokenUseCase refreshTokenUseCase, LogoutUseCase logoutUseCase)
    {
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
    }

    public sealed record LoginRequest(string Email, string Senha);
    public sealed record RefreshTokenRequest(string RefreshToken);

    [HttpPost("login")]
    [EnableRateLimiting(LoginRateLimitOptions.PolicyName)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _loginUseCase.ExecutarAsync(new LoginInput(request.Email, request.Senha), cancellationToken);

        if (!resultado.Sucesso)
            return Unauthorized(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(LoginRateLimitOptions.PolicyName)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _refreshTokenUseCase.ExecutarAsync(new RefreshTokenInput(request.RefreshToken), cancellationToken);

        if (!resultado.Sucesso)
            return Unauthorized(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    /// <summary>Encerra a sessão revogando o refresh token. Sempre responde 204, mesmo com token desconhecido.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _logoutUseCase.ExecutarAsync(new LogoutInput(request.RefreshToken), cancellationToken);
        return NoContent();
    }
}
