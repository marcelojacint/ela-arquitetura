using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class EntregaRepository : IEntregaRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public EntregaRepository(ElaArquiteturaDbContext context) => _context = context;

    public async Task<IReadOnlyCollection<Entrega>> ListarPorProjetoAsync(Guid projetoId, CancellationToken cancellationToken)
        => await _context.Entregas
            .Where(e => e.ProjetoId == projetoId)
            .OrderByDescending(e => e.DataEnvio)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Entrega entrega, CancellationToken cancellationToken)
    {
        await _context.Entregas.AddAsync(entrega, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
