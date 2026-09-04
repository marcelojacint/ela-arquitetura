using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Cliente>> BuscarAsync(string? termo, CancellationToken cancellationToken);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
}
