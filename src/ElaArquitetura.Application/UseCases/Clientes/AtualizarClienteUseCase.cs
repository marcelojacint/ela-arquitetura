using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record AtualizarClienteInput(Guid Id, string Nome, string Telefone, string? Email, string? Endereco);

public sealed class AtualizarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public AtualizarClienteUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<UseCaseResult<ClienteOutput>> ExecutarAsync(AtualizarClienteInput input, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (cliente is null)
            return UseCaseResult<ClienteOutput>.Falha(new[] { "Cliente não encontrado." });

        cliente.Atualizar(input.Nome, input.Telefone, input.Email, input.Endereco);
        if (!cliente.IsValid)
            return UseCaseResult<ClienteOutput>.Falha(cliente.Notifications.Select(n => n.Mensagem));

        await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

        return UseCaseResult<ClienteOutput>.Ok(ClienteOutput.DeCliente(cliente));
    }
}
