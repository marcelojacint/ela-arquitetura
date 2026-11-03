using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Checklist;

/// <summary>EtapaId opcional: sem ele o item vai para a etapa atual; com ele, para uma etapa já alcançada.</summary>
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

            var etapaAtual = await _etapaRepository.ObterPorIdAsync(projeto.EtapaAtualId, cancellationToken);
            if (etapaAtual is not null && etapa.Ordem > etapaAtual.Ordem)
                return UseCaseResult<ChecklistItemCriadoOutput>.Falha(new[] { "Só é possível adicionar itens em etapas já alcançadas pelo projeto." });

            etapaId = etapa.Id;
        }

        var item = new ChecklistItem(projeto.Id, etapaId, input.Descricao, input.SubEtapaId);
        await _checklistItemRepository.AdicionarAsync(item, cancellationToken);

        return UseCaseResult<ChecklistItemCriadoOutput>.Ok(
            new ChecklistItemCriadoOutput(item.Id, item.ProjetoId, item.EtapaId, item.SubEtapaId, item.Descricao, item.Concluido));
    }
}
