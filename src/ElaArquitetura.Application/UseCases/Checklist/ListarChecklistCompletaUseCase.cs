using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Etapas;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record EtapaDoChecklistOutput(
    Guid EtapaId,
    string Nome,
    int Ordem,
    bool Opcional,
    bool Final,
    bool Atual,
    bool Concluida,
    IReadOnlyCollection<SubEtapaOutput> SubEtapas,
    IReadOnlyCollection<ChecklistItemDetalheOutput> Itens);

/// <summary>
/// Checklist do projeto com todas as etapas (a tela mostra tudo de uma vez).
/// Os itens de todas as etapas já existem desde a abertura do projeto, inclusive os das etapas futuras.
/// </summary>
public sealed class ListarChecklistCompletaUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly ISubEtapaRepository _subEtapaRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;

    public ListarChecklistCompletaUseCase(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        ISubEtapaRepository subEtapaRepository,
        IChecklistItemRepository checklistItemRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _subEtapaRepository = subEtapaRepository;
        _checklistItemRepository = checklistItemRepository;
    }

    public async Task<UseCaseResult<IReadOnlyCollection<EtapaDoChecklistOutput>>> ExecutarAsync(Guid projetoId, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(projetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<IReadOnlyCollection<EtapaDoChecklistOutput>>.Falha(new[] { "Projeto não encontrado." });

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);
        var subEtapas = await _subEtapaRepository.ListarTodasAsync(cancellationToken);
        var itens = await _checklistItemRepository.ListarPorProjetoAsync(projeto.Id, cancellationToken);

        var ordemAtual = etapas.FirstOrDefault(e => e.Id == projeto.EtapaAtualId)?.Ordem ?? 0;

        // Itens das sub-etapas na ordem do fluxo; os avulsos vêm depois.
        var ordemSubEtapa = subEtapas.ToDictionary(s => s.Id, s => s.Ordem);
        int OrdemDoItem(Guid? subEtapaId) =>
            subEtapaId is not null && ordemSubEtapa.TryGetValue(subEtapaId.Value, out var ordem) ? ordem : int.MaxValue;
        var projetoConcluido = projeto.Status == StatusProjeto.Concluido;

        var saida = etapas
            .OrderBy(etapa => etapa.Ordem)
            .Select(etapa => new EtapaDoChecklistOutput(
                etapa.Id,
                etapa.Nome,
                etapa.Ordem,
                etapa.Opcional,
                etapa.Final,
                etapa.Id == projeto.EtapaAtualId,
                projetoConcluido || etapa.Ordem < ordemAtual,
                subEtapas
                    .Where(s => s.EtapaId == etapa.Id)
                    .OrderBy(s => s.Ordem)
                    .Select(s => new SubEtapaOutput(s.Id, s.Nome, s.Ordem))
                    .ToList(),
                itens
                    .Where(i => i.EtapaId == etapa.Id)
                    .OrderBy(i => OrdemDoItem(i.SubEtapaId))
                    .Select(i => new ChecklistItemDetalheOutput(i.Id, i.EtapaId, i.SubEtapaId, i.Descricao, i.Concluido, i.ConcluidoPor, i.DataConclusao))
                    .ToList()))
            .ToList();

        return UseCaseResult<IReadOnlyCollection<EtapaDoChecklistOutput>>.Ok(saida);
    }
}
