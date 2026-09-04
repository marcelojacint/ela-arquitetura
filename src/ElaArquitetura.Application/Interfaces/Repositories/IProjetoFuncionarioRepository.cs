using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IProjetoFuncionarioRepository
{
    Task<ProjetoFuncionario?> ObterAsync(Guid projetoId, Guid funcionarioId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjetoFuncionario>> ListarPorProjetoAsync(Guid projetoId, CancellationToken cancellationToken);
    Task AdicionarAsync(ProjetoFuncionario projetoFuncionario, CancellationToken cancellationToken);
    Task RemoverAsync(ProjetoFuncionario projetoFuncionario, CancellationToken cancellationToken);
}
