using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface ISubEtapaRepository
{
    Task<IReadOnlyCollection<SubEtapa>> ListarTodasAsync(CancellationToken cancellationToken);
}
