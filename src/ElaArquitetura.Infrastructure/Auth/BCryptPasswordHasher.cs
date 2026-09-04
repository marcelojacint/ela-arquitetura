using ElaArquitetura.Application.Interfaces.Auth;

namespace ElaArquitetura.Infrastructure.Auth;

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: WorkFactor);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
