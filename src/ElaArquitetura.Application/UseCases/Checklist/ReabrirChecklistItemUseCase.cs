using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record ReabrirChecklistItemInput(Guid ChecklistItemId);

public sealed class ReabrirChecklistItemUseCase
{
    private readonly IChecklistItemRepository _checklistItemRepository;

    public ReabrirChecklistItemUseCase(IChecklistItemRepository checklistItemRepository) => _checklistItemRepository = checklistItemRepository;

    public async Task<UseCaseResult<ChecklistItemOutput>> ExecutarAsync(ReabrirChecklistItemInput input, CancellationToken cancellationToken)
    {
        var item = await _checklistItemRepository.ObterPorIdAsync(input.ChecklistItemId, cancellationToken);
        if (item is null)
            return UseCaseResult<ChecklistItemOutput>.Falha(new[] { "Item de checklist não encontrado." });

        item.Reabrir();
        await _checklistItemRepository.AtualizarAsync(item, cancellationToken);

        return UseCaseResult<ChecklistItemOutput>.Ok(new ChecklistItemOutput(item.Id, item.Concluido, EtapaPodeAvancar: false));
    }
}
