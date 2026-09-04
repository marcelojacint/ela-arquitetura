using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class ProjetoFuncionarioRepository : IProjetoFuncionarioRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public ProjetoFuncionarioRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<ProjetoFuncionario?> ObterAsync(Guid projetoId, Guid funcionarioId, CancellationToken cancellationToken)
        => _context.ProjetoFuncionarios
            .FirstOrDefaultAsync(pf => pf.ProjetoId == projetoId && pf.FuncionarioId == funcionarioId, cancellationToken);

    public async Task<IReadOnlyCollection<ProjetoFuncionario>> ListarPorProjetoAsync(Guid projetoId, CancellationToken cancellationToken)
        => await _context.ProjetoFuncionarios
            .Where(pf => pf.ProjetoId == projetoId)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(ProjetoFuncionario projetoFuncionario, CancellationToken cancellationToken)
    {
        await _context.ProjetoFuncionarios.AddAsync(projetoFuncionario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(ProjetoFuncionario projetoFuncionario, CancellationToken cancellationToken)
    {
        _context.ProjetoFuncionarios.Remove(projetoFuncionario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
