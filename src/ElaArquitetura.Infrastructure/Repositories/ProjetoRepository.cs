using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class ProjetoRepository : IProjetoRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public ProjetoRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Projetos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Projeto>> ListarAsync(StatusProjeto? status, Guid? etapaId, CancellationToken cancellationToken)
    {
        var query = _context.Projetos.AsQueryable();

        if (status is not null)
            query = query.Where(p => p.Status == status);

        if (etapaId is not null)
            query = query.Where(p => p.EtapaAtualId == etapaId);

        return await query.OrderByDescending(p => p.DataInicio).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken)
    {
        await _context.Projetos.AddAsync(projeto, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Projeto projeto, CancellationToken cancellationToken)
    {
        _context.Projetos.Update(projeto);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
