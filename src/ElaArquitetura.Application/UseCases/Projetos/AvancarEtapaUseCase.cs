using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record AvancarEtapaInput(Guid ProjetoId);

/// <summary>
/// Avança para a próxima etapa e, como o checklist de todas as etapas já existe desde a abertura do projeto,
/// continua avançando enquanto a nova etapa atual já estiver com todos os itens concluídos
/// (itens marcados antes da hora não deixam o projeto parado numa etapa já cumprida).
/// </summary>
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

        while (!proximaEtapa.Final)
        {
            var checklistDaNovaEtapa = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projeto.Id, proximaEtapa.Id, cancellationToken);
            if (checklistDaNovaEtapa.Count == 0 || !checklistDaNovaEtapa.All(item => item.Concluido))
                break;

            var etapaSeguinte = await _etapaRepository.ObterProximaEtapaAsync(proximaEtapa, cancellationToken);
            if (etapaSeguinte is null)
                break;

            projeto.AvancarEtapa(proximaEtapa, etapaSeguinte, checklistDaNovaEtapa);
            proximaEtapa = etapaSeguinte;
        }

        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);

        _logger.LogInformation(
            "Projeto {ProjetoId} avancou da etapa {EtapaAnteriorId} para {EtapaNovaId}",
            projeto.Id, etapaAtual.Id, proximaEtapa.Id);

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas));
    }
}
