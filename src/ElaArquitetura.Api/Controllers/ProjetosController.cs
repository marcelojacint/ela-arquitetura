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
    private readonly AtualizarProjetoUseCase _atualizarProjetoUseCase;
    private readonly ExcluirProjetoUseCase _excluirProjetoUseCase;
    private readonly DefinirLinkDriveUseCase _definirLinkDriveUseCase;
    private readonly AvancarEtapaUseCase _avancarEtapaUseCase;
    private readonly ConcluirProjetoUseCase _concluirProjetoUseCase;
    private readonly ReabrirProjetoUseCase _reabrirProjetoUseCase;
    private readonly AtribuirFuncionarioAoProjetoUseCase _atribuirFuncionarioAoProjetoUseCase;
    private readonly RemoverFuncionarioDoProjetoUseCase _removerFuncionarioDoProjetoUseCase;
    private readonly ListarChecklistDaEtapaAtualUseCase _listarChecklistDaEtapaAtualUseCase;
    private readonly ListarChecklistCompletaUseCase _listarChecklistCompletaUseCase;
    private readonly CriarChecklistItemUseCase _criarChecklistItemUseCase;
    private readonly RegistrarEntregaUseCase _registrarEntregaUseCase;
    private readonly ListarEntregasDoProjetoUseCase _listarEntregasDoProjetoUseCase;

    public ProjetosController(
        CriarProjetoUseCase criarProjetoUseCase,
        ListarProjetosUseCase listarProjetosUseCase,
        ObterProjetoPorIdUseCase obterProjetoPorIdUseCase,
        AtualizarProjetoUseCase atualizarProjetoUseCase,
        ExcluirProjetoUseCase excluirProjetoUseCase,
        DefinirLinkDriveUseCase definirLinkDriveUseCase,
        AvancarEtapaUseCase avancarEtapaUseCase,
        ConcluirProjetoUseCase concluirProjetoUseCase,
        ReabrirProjetoUseCase reabrirProjetoUseCase,
        AtribuirFuncionarioAoProjetoUseCase atribuirFuncionarioAoProjetoUseCase,
        RemoverFuncionarioDoProjetoUseCase removerFuncionarioDoProjetoUseCase,
        ListarChecklistDaEtapaAtualUseCase listarChecklistDaEtapaAtualUseCase,
        ListarChecklistCompletaUseCase listarChecklistCompletaUseCase,
        CriarChecklistItemUseCase criarChecklistItemUseCase,
        RegistrarEntregaUseCase registrarEntregaUseCase,
        ListarEntregasDoProjetoUseCase listarEntregasDoProjetoUseCase)
    {
        _criarProjetoUseCase = criarProjetoUseCase;
        _listarProjetosUseCase = listarProjetosUseCase;
        _obterProjetoPorIdUseCase = obterProjetoPorIdUseCase;
        _atualizarProjetoUseCase = atualizarProjetoUseCase;
        _excluirProjetoUseCase = excluirProjetoUseCase;
        _definirLinkDriveUseCase = definirLinkDriveUseCase;
        _avancarEtapaUseCase = avancarEtapaUseCase;
        _concluirProjetoUseCase = concluirProjetoUseCase;
        _reabrirProjetoUseCase = reabrirProjetoUseCase;
        _atribuirFuncionarioAoProjetoUseCase = atribuirFuncionarioAoProjetoUseCase;
        _removerFuncionarioDoProjetoUseCase = removerFuncionarioDoProjetoUseCase;
        _listarChecklistDaEtapaAtualUseCase = listarChecklistDaEtapaAtualUseCase;
        _listarChecklistCompletaUseCase = listarChecklistCompletaUseCase;
        _criarChecklistItemUseCase = criarChecklistItemUseCase;
        _registrarEntregaUseCase = registrarEntregaUseCase;
        _listarEntregasDoProjetoUseCase = listarEntregasDoProjetoUseCase;
    }

    public sealed record CriarProjetoRequest(Guid ClienteId, string Titulo, string? EnderecoObra, Guid? FuncionarioResponsavelId);
    public sealed record AtualizarProjetoRequest(string Titulo, string? EnderecoObra, Guid? FuncionarioResponsavelId);
    public sealed record DefinirLinkDriveRequest(string LinkDrive);
    public sealed record AlterarStatusRequest(StatusProjeto Status);
    public sealed record AtribuirFuncionarioRequest(Guid FuncionarioId, string? PapelNoProjeto);
    public sealed record CriarChecklistItemRequest(string Descricao, Guid? SubEtapaId, Guid? EtapaId);
    public sealed record RegistrarEntregaRequest(string LinkDrive, bool NotificarWhatsApp);

    [HttpPost]
    public async Task<IActionResult> Criar(CriarProjetoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarProjetoUseCase.ExecutarAsync(new CriarProjetoInput(request.ClienteId, request.Titulo, request.EnderecoObra, request.FuncionarioResponsavelId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] StatusProjeto? status, [FromQuery] Guid? etapaId, [FromQuery] Guid? clienteId, CancellationToken cancellationToken)
    {
        var projetos = await _listarProjetosUseCase.ExecutarAsync(new ListarProjetosInput(status, etapaId, clienteId), cancellationToken);
        return Ok(projetos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var projeto = await _obterProjetoPorIdUseCase.ExecutarAsync(id, cancellationToken);
        return projeto is null ? NotFound() : Ok(projeto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarProjetoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _atualizarProjetoUseCase.ExecutarAsync(
            new AtualizarProjetoInput(id, request.Titulo, request.EnderecoObra, request.FuncionarioResponsavelId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _excluirProjetoUseCase.ExecutarAsync(new ExcluirProjetoInput(id), cancellationToken);

        return resultado switch
        {
            ResultadoExclusaoProjeto.NaoEncontrado => NotFound(new { erros = new[] { "Projeto não encontrado." } }),
            ResultadoExclusaoProjeto.UltimoProjetoDoCliente => BadRequest(new { erros = new[] { ExcluirProjetoUseCase.MensagemUltimoProjeto } }),
            _ => NoContent()
        };
    }

    [HttpPut("{id:guid}/link-drive")]
    public async Task<IActionResult> DefinirLinkDrive(Guid id, DefinirLinkDriveRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LinkDrive))
            return BadRequest(new { erros = new[] { "Informe o link do Drive." } });

        return await ResponderLinkDriveAsync(new DefinirLinkDriveInput(id, request.LinkDrive), cancellationToken);
    }

    [HttpDelete("{id:guid}/link-drive")]
    public Task<IActionResult> RemoverLinkDrive(Guid id, CancellationToken cancellationToken)
        => ResponderLinkDriveAsync(new DefinirLinkDriveInput(id, null), cancellationToken);

    private async Task<IActionResult> ResponderLinkDriveAsync(DefinirLinkDriveInput input, CancellationToken cancellationToken)
    {
        var resultado = await _definirLinkDriveUseCase.ExecutarAsync(input, cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
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

    [HttpGet("{id:guid}/checklist/completa")]
    public async Task<IActionResult> ListarChecklistCompleta(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _listarChecklistCompletaUseCase.ExecutarAsync(id, cancellationToken);

        if (!resultado.Sucesso)
            return NotFound(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpPost("{id:guid}/checklist")]
    public async Task<IActionResult> CriarItemChecklist(Guid id, CriarChecklistItemRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarChecklistItemUseCase.ExecutarAsync(
            new CriarChecklistItemInput(id, request.Descricao, request.SubEtapaId, request.EtapaId), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpGet("{id:guid}/entregas")]
    public async Task<IActionResult> ListarEntregas(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _listarEntregasDoProjetoUseCase.ExecutarAsync(id, cancellationToken);

        if (!resultado.Sucesso)
            return NotFound(new { erros = resultado.Erros });

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
