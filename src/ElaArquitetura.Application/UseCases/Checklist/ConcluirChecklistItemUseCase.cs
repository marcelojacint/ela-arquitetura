using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Checklist;

public sealed record ConcluirChecklistItemInput(Guid ChecklistItemId, Guid FuncionarioId);

public sealed record ChecklistItemOutput(Guid Id, bool Concluido, bool EtapaPodeAvancar);

/// <summary>
/// RF05/RF16 — marca o item e registra quem concluiu; também informa se, com essa marcação,
/// o checklist obrigatório da etapa já ficou completo (para o app habilitar o botão de avançar).
/// </summary>
public sealed class ConcluirChecklistItemUseCase
{
    private readonly IChecklistItemRepository _checklistItemRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;

    public ConcluirChecklistItemUseCase(
        IChecklistItemRepository checklistItemRepository,
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository)
    {
        _checklistItemRepository = checklistItemRepository;
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<UseCaseResult<ChecklistItemOutput>> ExecutarAsync(ConcluirChecklistItemInput input, CancellationToken cancellationToken)
    {
        var item = await _checklistItemRepository.ObterPorIdAsync(input.ChecklistItemId, cancellationToken);
        if (item is null)
            return UseCaseResult<ChecklistItemOutput>.Falha(new[] { "Item de checklist não encontrado." });

        item.Concluir(input.FuncionarioId);
        await _checklistItemRepository.AtualizarAsync(item, cancellationToken);

        var projeto = await _projetoRepository.ObterPorIdAsync(item.ProjetoId, cancellationToken);
        var etapaAtual = await _etapaRepository.ObterPorIdAsync(item.EtapaId, cancellationToken);

        var podeAvancar = false;
        if (projeto is not null && etapaAtual is not null && etapaAtual.Id == projeto.EtapaAtualId)
        {
            var checklist = await _checklistItemRepository.ListarPorProjetoEEtapaAsync(projeto.Id, etapaAtual.Id, cancellationToken);
            podeAvancar = projeto.PodeAvancarEtapa(etapaAtual, checklist);
        }

        return UseCaseResult<ChecklistItemOutput>.Ok(new ChecklistItemOutput(item.Id, item.Concluido, podeAvancar));
    }
}
