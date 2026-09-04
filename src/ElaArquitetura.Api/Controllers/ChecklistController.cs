using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElaArquitetura.Application.UseCases.Checklist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/checklist")]
public class ChecklistController : ControllerBase
{
    private readonly ConcluirChecklistItemUseCase _concluirChecklistItemUseCase;
    private readonly ReabrirChecklistItemUseCase _reabrirChecklistItemUseCase;

    public ChecklistController(
        ConcluirChecklistItemUseCase concluirChecklistItemUseCase,
        ReabrirChecklistItemUseCase reabrirChecklistItemUseCase)
    {
        _concluirChecklistItemUseCase = concluirChecklistItemUseCase;
        _reabrirChecklistItemUseCase = reabrirChecklistItemUseCase;
    }

    [HttpPatch("{itemId:guid}/concluir")]
    public async Task<IActionResult> Concluir(Guid itemId, CancellationToken cancellationToken)
    {
        var funcionarioId = ObterFuncionarioIdAutenticado();

        var resultado = await _concluirChecklistItemUseCase.ExecutarAsync(
            new ConcluirChecklistItemInput(itemId, funcionarioId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPatch("{itemId:guid}/reabrir")]
    public async Task<IActionResult> Reabrir(Guid itemId, CancellationToken cancellationToken)
    {
        var resultado = await _reabrirChecklistItemUseCase.ExecutarAsync(new ReabrirChecklistItemInput(itemId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    private Guid ObterFuncionarioIdAutenticado()
        => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
