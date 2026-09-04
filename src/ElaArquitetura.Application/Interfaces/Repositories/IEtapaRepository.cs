using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IEtapaRepository
{
    Task<Etapa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Etapa> ObterPrimeiraEtapaAsync(CancellationToken cancellationToken);
    Task<Etapa?> ObterProximaEtapaAsync(Etapa etapaAtual, CancellationToken cancellationToken);
}
