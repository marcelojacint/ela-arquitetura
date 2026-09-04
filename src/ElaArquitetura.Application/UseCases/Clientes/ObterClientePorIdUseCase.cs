using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed class ObterClientePorIdUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public ObterClientePorIdUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<ClienteOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        return cliente is null ? null : ClienteOutput.DeCliente(cliente);
    }
}
