using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed class ObterClientePorIdUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProjetoRepository _projetoRepository;

    public ObterClientePorIdUseCase(IClienteRepository clienteRepository, IProjetoRepository projetoRepository)
    {
        _clienteRepository = clienteRepository;
        _projetoRepository = projetoRepository;
    }

    private async Task<int> ContarProjetosAsync(Guid clienteId, CancellationToken cancellationToken)
        => (await _projetoRepository.ContarPorClientesAsync(new[] { clienteId }, cancellationToken)).GetValueOrDefault(clienteId);

    public async Task<ClienteOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        return cliente is null ? null : ClienteOutput.DeCliente(cliente, await ContarProjetosAsync(cliente.Id, cancellationToken));
    }
}
