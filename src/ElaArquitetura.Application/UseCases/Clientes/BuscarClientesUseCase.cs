using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record BuscarClientesInput(string? Termo);

/// <summary>RF09 — busca por nome, telefone ou e-mail em um único termo.</summary>
public sealed class BuscarClientesUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProjetoRepository _projetoRepository;

    public BuscarClientesUseCase(IClienteRepository clienteRepository, IProjetoRepository projetoRepository)
    {
        _clienteRepository = clienteRepository;
        _projetoRepository = projetoRepository;
    }

    public async Task<IReadOnlyCollection<ClienteOutput>> ExecutarAsync(BuscarClientesInput input, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.BuscarAsync(input.Termo, cancellationToken);
        var projetosPorCliente = await _projetoRepository.ContarPorClientesAsync(clientes.Select(c => c.Id).ToList(), cancellationToken);

        return clientes
            .Select(c => ClienteOutput.DeCliente(c, projetosPorCliente.GetValueOrDefault(c.Id)))
            .ToList();
    }
}
