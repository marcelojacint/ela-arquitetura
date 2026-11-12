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

    public async Task<IReadOnlyCollection<Projeto>> ListarAsync(StatusProjeto? status, Guid? etapaId, Guid? clienteId, CancellationToken cancellationToken)
    {
        var query = _context.Projetos.AsQueryable();

        if (status is not null)
            query = query.Where(p => p.Status == status);

        if (etapaId is not null)
            query = query.Where(p => p.EtapaAtualId == etapaId);

        if (clienteId is not null)
            query = query.Where(p => p.ClienteId == clienteId);

        return await query.OrderByDescending(p => p.DataInicio).ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteComTituloAsync(Guid clienteId, string titulo, Guid? ignorarProjetoId, CancellationToken cancellationToken)
    {
        var tituloNormalizado = titulo.Trim().ToLower();

        return _context.Projetos.AnyAsync(
            p => p.ClienteId == clienteId
                && p.Titulo.ToLower() == tituloNormalizado
                && (ignorarProjetoId == null || p.Id != ignorarProjetoId),
            cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> ContarPorClientesAsync(IReadOnlyCollection<Guid> clienteIds, CancellationToken cancellationToken)
        => await _context.Projetos
            .Where(p => clienteIds.Contains(p.ClienteId))
            .GroupBy(p => p.ClienteId)
            .Select(g => new { ClienteId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.ClienteId, x => x.Quantidade, cancellationToken);

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

    /// <summary>Checklist, entregas e vínculos com funcionários saem junto (cascade no banco).</summary>
    public async Task RemoverAsync(Projeto projeto, CancellationToken cancellationToken)
    {
        _context.Projetos.Remove(projeto);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
