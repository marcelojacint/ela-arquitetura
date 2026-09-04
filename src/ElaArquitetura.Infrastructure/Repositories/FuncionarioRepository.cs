using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public FuncionarioRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Funcionarios.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Funcionario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken)
        => _context.Funcionarios.FirstOrDefaultAsync(f => f.Email == email, cancellationToken);

    public async Task<IReadOnlyCollection<Funcionario>> ListarAsync(CancellationToken cancellationToken)
        => await _context.Funcionarios.OrderBy(f => f.Nome).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Funcionario funcionario, CancellationToken cancellationToken)
    {
        await _context.Funcionarios.AddAsync(funcionario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
