using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record DesativarClienteInput(Guid Id);

public sealed class DesativarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public DesativarClienteUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<UseCaseResult<bool>> ExecutarAsync(DesativarClienteInput input, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(input.Id, cancellationToken);
        if (cliente is null)
            return UseCaseResult<bool>.Falha(new[] { "Cliente não encontrado." });

        cliente.Desativar();
        await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

        return UseCaseResult<bool>.Ok(true);
    }
}
