namespace ElaArquitetura.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
