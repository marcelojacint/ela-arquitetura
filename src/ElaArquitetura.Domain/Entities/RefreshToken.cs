namespace ElaArquitetura.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public bool Revogado { get; private set; }

    protected RefreshToken()
    {
    }

    public RefreshToken(Guid funcionarioId, string tokenHash, DateTime expiraEm)
    {
        Id = Guid.NewGuid();
        FuncionarioId = funcionarioId;
        TokenHash = tokenHash;
        CriadoEm = DateTime.UtcNow;
        ExpiraEm = expiraEm;
        Revogado = false;
    }

    public bool EstaValido() => !Revogado && ExpiraEm > DateTime.UtcNow;

    public void Revogar() => Revogado = true;
}
