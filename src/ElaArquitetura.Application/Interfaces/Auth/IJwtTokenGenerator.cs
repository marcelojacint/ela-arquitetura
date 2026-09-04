using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    string GerarToken(Funcionario funcionario);
}
