using ElaArquitetura.Application.Interfaces.Auth;

namespace ElaArquitetura.Infrastructure.Auth;

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    /// <summary>Hash de uma senha aleatória, com o mesmo custo dos reais, usado quando não há hash para comparar.</summary>
    private static readonly Lazy<string> HashFicticio =
        new(() => BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), workFactor: WorkFactor));

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: WorkFactor);

    public bool Verificar(string senha, string? hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            BCrypt.Net.BCrypt.Verify(senha, HashFicticio.Value);
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
