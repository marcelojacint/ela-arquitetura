using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IEntregaRepository
{
    Task<IReadOnlyCollection<Entrega>> ListarPorProjetoAsync(Guid projetoId, CancellationToken cancellationToken);
    Task AdicionarAsync(Entrega entrega, CancellationToken cancellationToken);
}
