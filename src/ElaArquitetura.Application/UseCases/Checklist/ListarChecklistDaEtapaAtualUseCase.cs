using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record ChecklistItemDetalheOutput(
    Guid Id,
    Guid EtapaId,
    Guid? SubEtapaId,
    string Descricao,
    bool Concluido,
    Guid? ConcluidoPor,
    DateTime? DataConclusao);

public sealed class ListarChecklistDaEtapaAtualUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IChecklistItemRepository _checklistItemRepository;

    public ListarChecklistDaEtapaAtualUseCase(IProjetoRepository projetoRepository, IChecklistItemRepository checklistItemRepository)
    {
        _projetoRepository = projetoRepository;
        _checklistItemRepository = checklistItemRepository;
    }

    public async Task<UseCaseResult<IReadOnlyCollection<ChecklistItemDetalheOutput>>> ExecutarAsync(Guid projetoId, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(projetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<IReadOnlyCollection<ChecklistItemDetalheOutput>>.Falha(new[] { "Projeto não encontrado." });

        var itens = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projeto.Id, projeto.EtapaAtualId, cancellationToken);

        var saida = itens
            .Select(i => new ChecklistItemDetalheOutput(i.Id, i.EtapaId, i.SubEtapaId, i.Descricao, i.Concluido, i.ConcluidoPor, i.DataConclusao))
            .ToList();

        return UseCaseResult<IReadOnlyCollection<ChecklistItemDetalheOutput>>.Ok(saida);
    }
}
