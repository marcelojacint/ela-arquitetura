using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record CriarClienteInput(
    string Nome,
    string Telefone,
    string? Email,
    string? Endereco,
    string? Cpf,
    string? PontoReferencia);

public sealed class CriarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public CriarClienteUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<UseCaseResult<ClienteOutput>> ExecutarAsync(CriarClienteInput input, CancellationToken cancellationToken)
    {
        var cliente = Cliente.Criar(input.Nome, input.Telefone, input.Email, input.Endereco, input.Cpf, input.PontoReferencia);
        if (!cliente.IsValid)
            return UseCaseResult<ClienteOutput>.Falha(cliente.Notifications.Select(n => n.Mensagem));

        await _clienteRepository.AdicionarAsync(cliente, cancellationToken);

        return UseCaseResult<ClienteOutput>.Ok(ClienteOutput.DeCliente(cliente));
    }
}
