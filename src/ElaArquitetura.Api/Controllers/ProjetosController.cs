using ElaArquitetura.Application.UseCases.Projetos;
using ElaArquitetura.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projetos")]
public class ProjetosController : ControllerBase
{
    private readonly CriarProjetoUseCase _criarProjetoUseCase;
    private readonly ListarProjetosUseCase _listarProjetosUseCase;
    private readonly ObterProjetoPorIdUseCase _obterProjetoPorIdUseCase;

    public ProjetosController(
        CriarProjetoUseCase criarProjetoUseCase,
        ListarProjetosUseCase listarProjetosUseCase,
        ObterProjetoPorIdUseCase obterProjetoPorIdUseCase)
    {
        _criarProjetoUseCase = criarProjetoUseCase;
        _listarProjetosUseCase = listarProjetosUseCase;
        _obterProjetoPorIdUseCase = obterProjetoPorIdUseCase;
    }

    public sealed record CriarProjetoRequest(Guid ClienteId, string Titulo);

    [HttpPost]
    public async Task<IActionResult> Criar(CriarProjetoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarProjetoUseCase.ExecutarAsync(new CriarProjetoInput(request.ClienteId, request.Titulo), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] StatusProjeto? status, [FromQuery] Guid? etapaId, CancellationToken cancellationToken)
    {
        var projetos = await _listarProjetosUseCase.ExecutarAsync(new ListarProjetosInput(status, etapaId), cancellationToken);
        return Ok(projetos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var projeto = await _obterProjetoPorIdUseCase.ExecutarAsync(id, cancellationToken);
        return projeto is null ? NotFound() : Ok(projeto);
    }
}
