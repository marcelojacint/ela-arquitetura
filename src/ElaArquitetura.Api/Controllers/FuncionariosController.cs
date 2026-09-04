using ElaArquitetura.Application.UseCases.Funcionarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/funcionarios")]
public class FuncionariosController : ControllerBase
{
    private readonly CriarFuncionarioUseCase _criarFuncionarioUseCase;
    private readonly ListarFuncionariosUseCase _listarFuncionariosUseCase;

    public FuncionariosController(CriarFuncionarioUseCase criarFuncionarioUseCase, ListarFuncionariosUseCase listarFuncionariosUseCase)
    {
        _criarFuncionarioUseCase = criarFuncionarioUseCase;
        _listarFuncionariosUseCase = listarFuncionariosUseCase;
    }

    public sealed record CriarFuncionarioRequest(string Nome, string Email, string Cargo, string Senha);

    [HttpPost]
    public async Task<IActionResult> Criar(CriarFuncionarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarFuncionarioUseCase.ExecutarAsync(
            new CriarFuncionarioInput(request.Nome, request.Email, request.Cargo, request.Senha), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var funcionarios = await _listarFuncionariosUseCase.ExecutarAsync(cancellationToken);
        return Ok(funcionarios);
    }
}
