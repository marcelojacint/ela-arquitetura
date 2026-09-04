using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IChecklistItemRepository
{
    Task<ChecklistItem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChecklistItem>> ListarPorProjetoEEtapaAsync(Guid projetoId, Guid etapaId, CancellationToken cancellationToken);
    Task AdicionarAsync(ChecklistItem item, CancellationToken cancellationToken);
    Task AtualizarAsync(ChecklistItem item, CancellationToken cancellationToken);
}
