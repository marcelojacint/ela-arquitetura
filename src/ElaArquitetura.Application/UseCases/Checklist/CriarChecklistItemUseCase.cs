using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record CriarChecklistItemInput(Guid ProjetoId, string Descricao, Guid? SubEtapaId);

public sealed record ChecklistItemCriadoOutput(Guid Id, Guid ProjetoId, Guid EtapaId, Guid? SubEtapaId, string Descricao, bool Concluido);

public sealed class CriarChecklistItemUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;

    public CriarChecklistItemUseCase(IProjetoRepository projetoRepository, IChecklistItemRepository checklistItemRepository)
    {
        _projetoRepository = projetoRepository;
        _checklistItemRepository = checklistItemRepository;
    }

    public async Task<UseCaseResult<ChecklistItemCriadoOutput>> ExecutarAsync(CriarChecklistItemInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Projeto não encontrado." });

        if (string.IsNullOrWhiteSpace(input.Descricao))
            return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Descrição do item é obrigatória." });

        var item = new ChecklistItem(projeto.Id, projeto.EtapaAtualId, input.Descricao, input.SubEtapaId);
        await _checklistItemRepository.AdicionarAsync(item, cancellationToken);

        return UseCaseResult<ChecklistItemCriadoOutput>.Ok(
            new ChecklistItemCriadoOutput(item.Id, item.ProjetoId, item.EtapaId, item.SubEtapaId, item.Descricao, item.Concluido));
    }
}
