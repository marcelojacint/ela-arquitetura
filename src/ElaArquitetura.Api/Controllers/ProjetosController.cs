using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Application.UseCases.Entregas;
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
    private readonly AvancarEtapaUseCase _avancarEtapaUseCase;
    private readonly ConcluirProjetoUseCase _concluirProjetoUseCase;
    private readonly ReabrirProjetoUseCase _reabrirProjetoUseCase;
    private readonly AtribuirFuncionarioAoProjetoUseCase _atribuirFuncionarioAoProjetoUseCase;
    private readonly RemoverFuncionarioDoProjetoUseCase _removerFuncionarioDoProjetoUseCase;
    private readonly ListarChecklistDaEtapaAtualUseCase _listarChecklistDaEtapaAtualUseCase;
    private readonly CriarChecklistItemUseCase _criarChecklistItemUseCase;
    private readonly RegistrarEntregaUseCase _registrarEntregaUseCase;

    public ProjetosController(
        CriarProjetoUseCase criarProjetoUseCase,
        ListarProjetosUseCase listarProjetosUseCase,
        ObterProjetoPorIdUseCase obterProjetoPorIdUseCase,
        AvancarEtapaUseCase avancarEtapaUseCase,
        ConcluirProjetoUseCase concluirProjetoUseCase,
        ReabrirProjetoUseCase reabrirProjetoUseCase,
        AtribuirFuncionarioAoProjetoUseCase atribuirFuncionarioAoProjetoUseCase,
        RemoverFuncionarioDoProjetoUseCase removerFuncionarioDoProjetoUseCase,
        ListarChecklistDaEtapaAtualUseCase listarChecklistDaEtapaAtualUseCase,
        CriarChecklistItemUseCase criarChecklistItemUseCase,
        RegistrarEntregaUseCase registrarEntregaUseCase)
    {
        _criarProjetoUseCase = criarProjetoUseCase;
        _listarProjetosUseCase = listarProjetosUseCase;
        _obterProjetoPorIdUseCase = obterProjetoPorIdUseCase;
        _avancarEtapaUseCase = avancarEtapaUseCase;
        _concluirProjetoUseCase = concluirProjetoUseCase;
        _reabrirProjetoUseCase = reabrirProjetoUseCase;
        _atribuirFuncionarioAoProjetoUseCase = atribuirFuncionarioAoProjetoUseCase;
        _removerFuncionarioDoProjetoUseCase = removerFuncionarioDoProjetoUseCase;
        _listarChecklistDaEtapaAtualUseCase = listarChecklistDaEtapaAtualUseCase;
        _criarChecklistItemUseCase = criarChecklistItemUseCase;
        _registrarEntregaUseCase = registrarEntregaUseCase;
    }

    public sealed record CriarProjetoRequest(Guid ClienteId, string Titulo);
    public sealed record AlterarStatusRequest(StatusProjeto Status);
    public sealed record AtribuirFuncionarioRequest(Guid FuncionarioId, string? PapelNoProjeto);
    public sealed record CriarChecklistItemRequest(string Descricao, Guid? SubEtapaId);
    public sealed record RegistrarEntregaRequest(string LinkDrive, bool NotificarWhatsApp);

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

    [HttpPatch("{id:guid}/avancar-etapa")]
    public async Task<IActionResult> AvancarEtapa(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _avancarEtapaUseCase.ExecutarAsync(new AvancarEtapaInput(id), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AlterarStatus(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken)
    {
        var resultado = request.Status == StatusProjeto.Concluido
            ? await _concluirProjetoUseCase.ExecutarAsync(new ConcluirProjetoInput(id), cancellationToken)
            : await _reabrirProjetoUseCase.ExecutarAsync(new ReabrirProjetoInput(id), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("{id:guid}/funcionarios")]
    public async Task<IActionResult> AtribuirFuncionario(Guid id, AtribuirFuncionarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _atribuirFuncionarioAoProjetoUseCase.ExecutarAsync(
            new AtribuirFuncionarioInput(id, request.FuncionarioId, request.PapelNoProjeto), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return NoContent();
    }

    [HttpDelete("{id:guid}/funcionarios/{funcionarioId:guid}")]
    public async Task<IActionResult> RemoverFuncionario(Guid id, Guid funcionarioId, CancellationToken cancellationToken)
    {
        var resultado = await _removerFuncionarioDoProjetoUseCase.ExecutarAsync(
            new RemoverFuncionarioInput(id, funcionarioId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return NoContent();
    }

    [HttpGet("{id:guid}/checklist")]
    public async Task<IActionResult> ObterChecklist(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _listarChecklistDaEtapaAtualUseCase.ExecutarAsync(id, cancellationToken);

        if (!resultado.Sucesso)
            return NotFound(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("{id:guid}/checklist")]
    public async Task<IActionResult> CriarItemChecklist(Guid id, CriarChecklistItemRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarChecklistItemUseCase.ExecutarAsync(
            new CriarChecklistItemInput(id, request.Descricao, request.SubEtapaId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("{id:guid}/entrega")]
    public async Task<IActionResult> RegistrarEntrega(Guid id, RegistrarEntregaRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _registrarEntregaUseCase.ExecutarAsync(
            new RegistrarEntregaInput(id, request.LinkDrive, request.NotificarWhatsApp), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }
}
