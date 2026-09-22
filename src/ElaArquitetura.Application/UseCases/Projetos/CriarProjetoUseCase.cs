using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record CriarProjetoInput(Guid ClienteId, string Titulo);

public sealed class CriarProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly ProvisionadorChecklist _provisionadorChecklist;

    public CriarProjetoUseCase(
        IProjetoRepository projetoRepository,
        IClienteRepository clienteRepository,
        IEtapaRepository etapaRepository,
        ProvisionadorChecklist provisionadorChecklist)
    {
        _projetoRepository = projetoRepository;
        _clienteRepository = clienteRepository;
        _etapaRepository = etapaRepository;
        _provisionadorChecklist = provisionadorChecklist;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(CriarProjetoInput input, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(input.ClienteId, cancellationToken);
        if (cliente is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Cliente não encontrado." });

        var etapaInicial = await _etapaRepository.ObterPrimeiraEtapaAsync(cancellationToken);

        var projeto = Projeto.Criar(cliente.Id, input.Titulo, etapaInicial);
        if (!projeto.IsValid)
            return UseCaseResult<ProjetoOutput>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        await _projetoRepository.AdicionarAsync(projeto, cancellationToken);
        await _provisionadorChecklist.ProvisionarAsync(projeto.Id, etapaInicial.Id, cancellationToken);

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas));
    }
}
