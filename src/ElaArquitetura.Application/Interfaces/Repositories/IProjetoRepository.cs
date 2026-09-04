using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IProjetoRepository
{
    Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Projeto>> ListarAsync(StatusProjeto? status, Guid? etapaId, CancellationToken cancellationToken);
    Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken);
    Task AtualizarAsync(Projeto projeto, CancellationToken cancellationToken);
}
