using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record AvancarEtapaInput(Guid ProjetoId);

public sealed class AvancarEtapaUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;
    private readonly ILogger<AvancarEtapaUseCase> _logger;

    public AvancarEtapaUseCase(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        IChecklistItemRepository checklistItemRepository,
        ILogger<AvancarEtapaUseCase> logger)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _checklistItemRepository = checklistItemRepository;
        _logger = logger;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(AvancarEtapaInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Projeto não encontrado." });

        var etapaAtual = await _etapaRepository.ObterPorIdAsync(projeto.EtapaAtualId, cancellationToken);
        if (etapaAtual is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Etapa atual do projeto não encontrada." });

        var proximaEtapa = await _etapaRepository.ObterProximaEtapaAsync(etapaAtual, cancellationToken);
        if (proximaEtapa is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Não há próxima etapa após a etapa atual." });

        var checklist = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projeto.Id, etapaAtual.Id, cancellationToken);

        projeto.AvancarEtapa(etapaAtual, proximaEtapa, checklist);
        if (!projeto.IsValid)
            return UseCaseResult<ProjetoOutput>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);

        _logger.LogInformation(
            "Projeto {ProjetoId} avancou da etapa {EtapaAnteriorId} para {EtapaNovaId}",
            projeto.Id, etapaAtual.Id, proximaEtapa.Id);

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas));
    }
}
