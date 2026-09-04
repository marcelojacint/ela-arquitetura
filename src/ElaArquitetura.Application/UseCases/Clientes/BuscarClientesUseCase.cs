using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record BuscarClientesInput(string? Termo);

/// <summary>RF09 — busca por nome, telefone ou e-mail em um único termo.</summary>
public sealed class BuscarClientesUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public BuscarClientesUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<IReadOnlyCollection<ClienteOutput>> ExecutarAsync(BuscarClientesInput input, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.BuscarAsync(input.Termo, cancellationToken);
        return clientes.Select(ClienteOutput.DeCliente).ToList();
    }
}
