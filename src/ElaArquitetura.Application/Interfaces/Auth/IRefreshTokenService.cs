namespace ElaArquitetura.Application.Interfaces.Auth;

public interface IRefreshTokenService
{
    string GerarToken();
    string Hash(string token);
}
