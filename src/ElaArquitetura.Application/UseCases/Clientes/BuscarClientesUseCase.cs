using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record BuscarClientesInput(string? Termo);

/// <summary>
/// RF09 — busca por nome, telefone ou e-mail em um único termo.
/// Só traz clientes com algum projeto em andamento: quem tem todos os projetos concluídos
/// aparece em "Concluídos" (ListarClientesConcluidosUseCase).
/// </summary>
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
        var projetos = await _projetoRepository.ListarPorClientesAsync(clientes.Select(c => c.Id).ToList(), cancellationToken);
        var projetosPorCliente = projetos.ToLookup(p => p.ClienteId);

        return clientes
            .Where(c => !TodosConcluidos(projetosPorCliente[c.Id]))
            .Select(c => ClienteOutput.DeCliente(c, projetosPorCliente[c.Id].Count()))
            .ToList();
    }

    /// <summary>Cliente sem projeto não conta como concluído.</summary>
    internal static bool TodosConcluidos(IEnumerable<Projeto> projetos)
        => projetos.Any() && projetos.All(p => p.Status == StatusProjeto.Concluido);
}
