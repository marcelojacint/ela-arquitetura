using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task AtualizarAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}
