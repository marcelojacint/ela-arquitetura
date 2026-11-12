using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Checklist;

/// <summary>
/// Item personalizado do cliente. EtapaId opcional: sem ele o item vai para a etapa atual; com ele, para a etapa informada
/// (qualquer etapa, já que o checklist do projeto inteiro existe desde a abertura).
/// </summary>
public sealed record CriarChecklistItemInput(Guid ProjetoId, string Descricao, Guid? SubEtapaId, Guid? EtapaId = null);

public sealed record ChecklistItemCriadoOutput(Guid Id, Guid ProjetoId, Guid EtapaId, Guid? SubEtapaId, string Descricao, bool Concluido);

public sealed class CriarChecklistItemUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;
    private readonly IEtapaRepository _etapaRepository;

    public CriarChecklistItemUseCase(
        IProjetoRepository projetoRepository,
        IChecklistItemRepository checklistItemRepository,
        IEtapaRepository etapaRepository)
    {
        _projetoRepository = projetoRepository;
        _checklistItemRepository = checklistItemRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<UseCaseResult<ChecklistItemCriadoOutput>> ExecutarAsync(CriarChecklistItemInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Projeto não encontrado." });

        if (string.IsNullOrWhiteSpace(input.Descricao))
            return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Descrição do item é obrigatória." });

        var etapaId = projeto.EtapaAtualId;
        if (input.EtapaId is not null && input.EtapaId != projeto.EtapaAtualId)
        {
            var etapa = await _etapaRepository.ObterPorIdAsync(input.EtapaId.Value, cancellationToken);
            if (etapa is null)
                return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Etapa não encontrada." });

            etapaId = etapa.Id;
        }

        var item = new ChecklistItem(projeto.Id, etapaId, input.Descricao, input.SubEtapaId);
        await _checklistItemRepository.AdicionarAsync(item, cancellationToken);

        return UseCaseResult<ChecklistItemCriadoOutput>.Ok(
            new ChecklistItemCriadoOutput(item.Id, item.ProjetoId, item.EtapaId, item.SubEtapaId, item.Descricao, item.Concluido));
    }
}
