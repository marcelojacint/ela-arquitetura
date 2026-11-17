using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public RefreshTokenRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken)
        => _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TentarRevogarAsync(Guid id, CancellationToken cancellationToken)
    {
        var afetados = await _context.RefreshTokens
            .Where(rt => rt.Id == id && !rt.Revogado)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revogado, true), cancellationToken);

        return afetados == 1;
    }

    public Task RevogarTodosDoFuncionarioAsync(Guid funcionarioId, CancellationToken cancellationToken)
        => _context.RefreshTokens
            .Where(rt => rt.FuncionarioId == funcionarioId && !rt.Revogado)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revogado, true), cancellationToken);

    public Task RemoverExpiradosDoFuncionarioAsync(Guid funcionarioId, CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        return _context.RefreshTokens
            .Where(rt => rt.FuncionarioId == funcionarioId && rt.ExpiraEm < agora)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
