namespace ElaArquitetura.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    string Hash(string senha);

    /// <summary>
    /// Confere a senha com o hash. Com <paramref name="hash"/> nulo (usuário inexistente ou sem senha)
    /// devolve false, mas gasta o mesmo tempo de uma verificação real, para que o tempo de resposta
    /// do login não revele quais e-mails estão cadastrados.
    /// </summary>
    bool Verificar(string senha, string? hash);
}
