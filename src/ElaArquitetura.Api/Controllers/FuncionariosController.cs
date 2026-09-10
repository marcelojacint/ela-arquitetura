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
    private readonly ObterFuncionarioPorIdUseCase _obterFuncionarioPorIdUseCase;
    private readonly AtualizarFuncionarioUseCase _atualizarFuncionarioUseCase;
    private readonly DesativarFuncionarioUseCase _desativarFuncionarioUseCase;

    public FuncionariosController(
        CriarFuncionarioUseCase criarFuncionarioUseCase,
        ListarFuncionariosUseCase listarFuncionariosUseCase,
        ObterFuncionarioPorIdUseCase obterFuncionarioPorIdUseCase,
        AtualizarFuncionarioUseCase atualizarFuncionarioUseCase,
        DesativarFuncionarioUseCase desativarFuncionarioUseCase)
    {
        _criarFuncionarioUseCase = criarFuncionarioUseCase;
        _listarFuncionariosUseCase = listarFuncionariosUseCase;
        _obterFuncionarioPorIdUseCase = obterFuncionarioPorIdUseCase;
        _atualizarFuncionarioUseCase = atualizarFuncionarioUseCase;
        _desativarFuncionarioUseCase = desativarFuncionarioUseCase;
    }

    public sealed record CriarFuncionarioRequest(string Nome, string Email, string Cargo, string Senha);
    public sealed record AtualizarFuncionarioRequest(string Nome, string Email, string Cargo);

    [HttpPost]
    public async Task<IActionResult> Criar(CriarFuncionarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarFuncionarioUseCase.ExecutarAsync(
            new CriarFuncionarioInput(request.Nome, request.Email, request.Cargo, request.Senha), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var funcionarios = await _listarFuncionariosUseCase.ExecutarAsync(cancellationToken);
        return Ok(funcionarios);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var funcionario = await _obterFuncionarioPorIdUseCase.ExecutarAsync(id, cancellationToken);
        return funcionario is null ? NotFound() : Ok(funcionario);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarFuncionarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _atualizarFuncionarioUseCase.ExecutarAsync(
            new AtualizarFuncionarioInput(id, request.Nome, request.Email, request.Cargo), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _desativarFuncionarioUseCase.ExecutarAsync(new DesativarFuncionarioInput(id), cancellationToken);

        if (!resultado.Sucesso)
            return NotFound(new { erros = resultado.Erros });

        return NoContent();
    }
}
