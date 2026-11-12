using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Checklist;

/// <summary>
/// Cria os itens do checklist a partir das sub-etapas de todas as etapas do fluxo.
/// O projeto já nasce com o checklist completo, para que o card do cliente abra com tudo pronto.
/// É idempotente: sub-etapas que já têm item no projeto são ignoradas.
/// </summary>
public sealed class ProvisionadorChecklist
{
    private readonly ISubEtapaRepository _subEtapaRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;

    public ProvisionadorChecklist(ISubEtapaRepository subEtapaRepository, IChecklistItemRepository checklistItemRepository)
    {
        _subEtapaRepository = subEtapaRepository;
        _checklistItemRepository = checklistItemRepository;
    }

    public async Task ProvisionarAsync(Guid projetoId, CancellationToken cancellationToken)
    {
        var subEtapas = await _subEtapaRepository.ListarTodasAsync(cancellationToken);
        if (subEtapas.Count == 0)
            return;

        var subEtapasJaProvisionadas = (await _checklistItemRepository.ListarPorProjetoAsync(projetoId, cancellationToken))
            .Where(item => item.SubEtapaId.HasValue)
            .Select(item => item.SubEtapaId!.Value)
            .ToHashSet();

        var novosItens = subEtapas
            .Where(subEtapa => !subEtapasJaProvisionadas.Contains(subEtapa.Id))
            .OrderBy(subEtapa => subEtapa.Ordem)
            .Select(subEtapa => new ChecklistItem(projetoId, subEtapa.EtapaId, subEtapa.Nome, subEtapa.Id))
            .ToList();

        if (novosItens.Count > 0)
            await _checklistItemRepository.AdicionarVariosAsync(novosItens, cancellationToken);
    }
}
