using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record RemoverChecklistItemInput(Guid ChecklistItemId);

/// <summary>
/// Remove um item avulso (criado à mão). Itens que vêm das sub-etapas fazem parte do fluxo e não podem ser removidos.
/// Devolve se a etapa atual já pode avançar sem o item removido.
/// </summary>
public sealed class RemoverChecklistItemUseCase
{
    private readonly IChecklistItemRepository _checklistItemRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;

    public RemoverChecklistItemUseCase(
        IChecklistItemRepository checklistItemRepository,
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository)
    {
        _checklistItemRepository = checklistItemRepository;
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<UseCaseResult<ChecklistItemOutput>> ExecutarAsync(RemoverChecklistItemInput input, CancellationToken cancellationToken)
    {
        var item = await _checklistItemRepository.ObterPorIdAsync(input.ChecklistItemId, cancellationToken);
        if (item is null)
            return UseCaseResult<ChecklistItemOutput>.Falha(new[] { "Item de checklist não encontrado." });

        if (item.SubEtapaId is not null)
            return UseCaseResult<ChecklistItemOutput>.Falha(new[] { "Somente itens adicionados manualmente podem ser removidos." });

        await _checklistItemRepository.RemoverAsync(item, cancellationToken);

        var podeAvancar = false;
        var projeto = await _projetoRepository.ObterPorIdAsync(item.ProjetoId, cancellationToken);
        var etapa = await _etapaRepository.ObterPorIdAsync(item.EtapaId, cancellationToken);

        if (projeto is not null && etapa is not null && etapa.Id == projeto.EtapaAtualId)
        {
            var checklist = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projeto.Id, etapa.Id, cancellationToken);
            podeAvancar = projeto.PodeAvancarEtapa(etapa, checklist);
        }

        return UseCaseResult<ChecklistItemOutput>.Ok(new ChecklistItemOutput(item.Id, item.Concluido, podeAvancar));
    }
}
