using ElaArquitetura.Application.UseCases.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly CriarClienteUseCase _criarClienteUseCase;
    private readonly AtualizarClienteUseCase _atualizarClienteUseCase;
    private readonly BuscarClientesUseCase _buscarClientesUseCase;
    private readonly ObterClientePorIdUseCase _obterClientePorIdUseCase;

    public ClientesController(
        CriarClienteUseCase criarClienteUseCase,
        AtualizarClienteUseCase atualizarClienteUseCase,
        BuscarClientesUseCase buscarClientesUseCase,
        ObterClientePorIdUseCase obterClientePorIdUseCase)
    {
        _criarClienteUseCase = criarClienteUseCase;
        _atualizarClienteUseCase = atualizarClienteUseCase;
        _buscarClientesUseCase = buscarClientesUseCase;
        _obterClientePorIdUseCase = obterClientePorIdUseCase;
    }

    public sealed record CriarClienteRequest(string Nome, string Telefone, string? Email, string? Endereco);

    public sealed record AtualizarClienteRequest(string Nome, string Telefone, string? Email, string? Endereco);

    [HttpPost]
    public async Task<IActionResult> Criar(CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _criarClienteUseCase.ExecutarAsync(
            new CriarClienteInput(request.Nome, request.Telefone, request.Email, request.Endereco), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? busca, CancellationToken cancellationToken)
    {
        var clientes = await _buscarClientesUseCase.ExecutarAsync(new BuscarClientesInput(busca), cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _obterClientePorIdUseCase.ExecutarAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _atualizarClienteUseCase.ExecutarAsync(
            new AtualizarClienteInput(id, request.Nome, request.Telefone, request.Email, request.Endereco), cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest(new { erros = resultado.Erros });

        return Ok(resultado.Dados);
    }
}
