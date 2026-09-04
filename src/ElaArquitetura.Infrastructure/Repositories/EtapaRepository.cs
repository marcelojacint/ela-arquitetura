using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class EtapaRepository : IEtapaRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public EtapaRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<Etapa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Etapas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Etapa> ObterPrimeiraEtapaAsync(CancellationToken cancellationToken)
        => await _context.Etapas.OrderBy(e => e.Ordem).FirstAsync(cancellationToken);

    public async Task<Etapa?> ObterProximaEtapaAsync(Etapa etapaAtual, CancellationToken cancellationToken)
        => await _context.Etapas
            .Where(e => e.Ordem > etapaAtual.Ordem)
            .OrderBy(e => e.Ordem)
            .FirstOrDefaultAsync(cancellationToken);
}
