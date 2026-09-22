using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed class ProvisionadorChecklist
{
    private readonly ISubEtapaRepository _subEtapaRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;

    public ProvisionadorChecklist(ISubEtapaRepository subEtapaRepository, IChecklistItemRepository checklistItemRepository)
    {
        _subEtapaRepository = subEtapaRepository;
        _checklistItemRepository = checklistItemRepository;
    }

    public async Task ProvisionarAsync(Guid projetoId, Guid etapaId, CancellationToken cancellationToken)
    {
        var subEtapasDaEtapa = (await _subEtapaRepository.ListarTodasAsync(cancellationToken))
            .Where(subEtapa => subEtapa.EtapaId == etapaId)
            .OrderBy(subEtapa => subEtapa.Ordem)
            .ToList();

        if (subEtapasDaEtapa.Count == 0)
            return;

        var itensExistentes = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projetoId, etapaId, cancellationToken);
        var subEtapasJaProvisionadas = itensExistentes
            .Where(item => item.SubEtapaId.HasValue)
            .Select(item => item.SubEtapaId!.Value)
            .ToHashSet();

        foreach (var subEtapa in subEtapasDaEtapa)
        {
            if (subEtapasJaProvisionadas.Contains(subEtapa.Id))
                continue;

            await _checklistItemRepository.AdicionarAsync(
                new ChecklistItem(projetoId, etapaId, subEtapa.Nome, subEtapa.Id), cancellationToken);
        }
    }
}
