using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IProjetoRepository
{
    Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Projeto>> ListarAsync(StatusProjeto? status, Guid? etapaId, Guid? clienteId, CancellationToken cancellationToken);
    Task<bool> ExisteComTituloAsync(Guid clienteId, string titulo, Guid? ignorarProjetoId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, int>> ContarPorClientesAsync(IReadOnlyCollection<Guid> clienteIds, CancellationToken cancellationToken);
    Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken);
    Task AtualizarAsync(Projeto projeto, CancellationToken cancellationToken);
    Task RemoverAsync(Projeto projeto, CancellationToken cancellationToken);
}
