using ElaArquitetura.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;

    public AuthController(LoginUseCase loginUseCase) => _loginUseCase = loginUseCase;

    public sealed record LoginRequest(string Email, string Senha);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _loginUseCase.ExecutarAsync(new LoginInput(request.Email, request.Senha), cancellationToken);

        if (!resultado.Sucesso)
            return Unauthorized(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }
}
