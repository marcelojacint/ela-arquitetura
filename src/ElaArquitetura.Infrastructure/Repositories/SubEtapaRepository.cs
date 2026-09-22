using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class SubEtapaRepository : ISubEtapaRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public SubEtapaRepository(ElaArquiteturaDbContext context) => _context = context;

    public async Task<IReadOnlyCollection<SubEtapa>> ListarTodasAsync(CancellationToken cancellationToken)
        => await _context.SubEtapas.OrderBy(s => s.Ordem).ToListAsync(cancellationToken);
}
