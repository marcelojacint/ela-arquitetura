using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record CriarProjetoInput(Guid ClienteId, string Titulo, string? EnderecoObra = null, Guid? FuncionarioResponsavelId = null);

/// <summary>
/// Projeto adicional para um cliente. O primeiro projeto já é aberto no cadastro do cliente.
/// </summary>
public sealed class CriarProjetoUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly AberturaDeProjeto _aberturaDeProjeto;

    public CriarProjetoUseCase(
        IClienteRepository clienteRepository,
        IEtapaRepository etapaRepository,
        AberturaDeProjeto aberturaDeProjeto)
    {
        _clienteRepository = clienteRepository;
        _etapaRepository = etapaRepository;
        _aberturaDeProjeto = aberturaDeProjeto;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(CriarProjetoInput input, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(input.ClienteId, cancellationToken);
        if (cliente is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Cliente não encontrado." });

        var abertura = await _aberturaDeProjeto.AbrirAsync(
            new DadosDoProjeto(cliente.Id, input.Titulo, input.EnderecoObra, input.FuncionarioResponsavelId), cancellationToken);
        if (!abertura.Sucesso)
            return UseCaseResult<ProjetoOutput>.Falha(abertura.Erros);
        var projeto = abertura.Dados!;

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas));
    }
}
