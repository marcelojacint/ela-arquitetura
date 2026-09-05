using System.Security.Cryptography;
using System.Text;
using ElaArquitetura.Application.Interfaces.Auth;

namespace ElaArquitetura.Infrastructure.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    public string GerarToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
