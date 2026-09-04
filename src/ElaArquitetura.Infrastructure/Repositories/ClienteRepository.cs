using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ElaArquiteturaDbContext _context;

    public ClienteRepository(ElaArquiteturaDbContext context) => _context = context;

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Cliente>> BuscarAsync(string? termo, CancellationToken cancellationToken)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var padrao = $"%{termo}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Nome, padrao) ||
                (c.Email != null && EF.Functions.ILike(c.Email, padrao)) ||
                (c.Telefone != null && EF.Functions.ILike(c.Telefone.Numero, padrao)));
        }

        return await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
