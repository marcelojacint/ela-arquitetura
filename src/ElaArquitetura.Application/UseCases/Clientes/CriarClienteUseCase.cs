using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Projetos;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record CriarClienteInput(
    string Nome,
    string Telefone,
    string? Email,
    string? Endereco,
    string? Cpf,
    string? PontoReferencia,
    /// <summary>Vai para o primeiro projeto: o endereço da obra é do projeto, não do cliente.</summary>
    string? EnderecoObra = null);

/// <summary>
/// Todo cliente cadastrado tem obrigatoriamente um projeto: o cadastro já abre o projeto
/// com o checklist de todas as etapas pronto.
/// </summary>
public sealed class CriarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly AberturaDeProjeto _aberturaDeProjeto;

    public CriarClienteUseCase(IClienteRepository clienteRepository, AberturaDeProjeto aberturaDeProjeto)
    {
        _clienteRepository = clienteRepository;
        _aberturaDeProjeto = aberturaDeProjeto;
    }

    public async Task<UseCaseResult<ClienteOutput>> ExecutarAsync(CriarClienteInput input, CancellationToken cancellationToken)
    {
        var cliente = Cliente.Criar(input.Nome, input.Telefone, input.Email, input.Endereco, input.Cpf, input.PontoReferencia);
        if (!cliente.IsValid)
            return UseCaseResult<ClienteOutput>.Falha(cliente.Notifications.Select(n => n.Mensagem));

        await _clienteRepository.AdicionarAsync(cliente, cancellationToken);
        var abertura = await _aberturaDeProjeto.AbrirAsync(new DadosDoProjeto(cliente.Id, cliente.Nome, input.EnderecoObra), cancellationToken);

        return UseCaseResult<ClienteOutput>.Ok(ClienteOutput.DeCliente(cliente, abertura.Sucesso ? 1 : 0));
    }
}
