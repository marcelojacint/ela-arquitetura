using ElaArquitetura.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;

    public AuthController(LoginUseCase loginUseCase, RefreshTokenUseCase refreshTokenUseCase)
    {
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
    }

    public sealed record LoginRequest(string Email, string Senha);
    public sealed record RefreshTokenRequest(string RefreshToken);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _loginUseCase.ExecutarAsync(new LoginInput(request.Email, request.Senha), cancellationToken);

        if (!resultado.Sucesso)
            return Unauthorized(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _refreshTokenUseCase.ExecutarAsync(new RefreshTokenInput(request.RefreshToken), cancellationToken);

        if (!resultado.Sucesso)
            return Unauthorized(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }
}
