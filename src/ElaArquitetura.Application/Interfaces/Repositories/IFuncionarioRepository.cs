using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Funcionario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Funcionario>> ListarAsync(CancellationToken cancellationToken);
    Task AdicionarAsync(Funcionario funcionario, CancellationToken cancellationToken);
}
