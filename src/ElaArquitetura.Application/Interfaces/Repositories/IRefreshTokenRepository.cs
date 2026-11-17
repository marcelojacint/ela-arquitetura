using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Revoga o token só se ele ainda não estiver revogado, numa única operação no banco.
    /// Devolve false se outra requisição chegou antes (o token já tinha sido usado).
    /// </summary>
    Task<bool> TentarRevogarAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Revoga todos os tokens ativos do funcionário (encerra todas as sessões dele).</summary>
    Task RevogarTodosDoFuncionarioAsync(Guid funcionarioId, CancellationToken cancellationToken);

    /// <summary>Apaga os tokens já expirados do funcionário, que não servem mais para nada.</summary>
    Task RemoverExpiradosDoFuncionarioAsync(Guid funcionarioId, CancellationToken cancellationToken);
}
