using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class ChecklistItemRepository : IChecklistItemRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public ChecklistItemRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<ChecklistItem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ChecklistItens.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<ChecklistItem>> ListarPorProjetoEEtapaAsync(Guid projetoId, Guid etapaId, CancellationToken cancellationToken)
        => await _context.ChecklistItens
            .Where(c => c.ProjetoId == projetoId && c.EtapaId == etapaId)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(ChecklistItem item, CancellationToken cancellationToken)
    {
        await _context.ChecklistItens.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(ChecklistItem item, CancellationToken cancellationToken)
    {
        _context.ChecklistItens.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
