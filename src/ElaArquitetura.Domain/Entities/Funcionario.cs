using ElaArquitetura.Domain.Common;

namespace ElaArquitetura.Domain.Entities;

public class Funcionario : Notifiable
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Cargo { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    protected Funcionario()
    {
    }

    public static Funcionario Criar(string nome, string email, string cargo, string senhaHash)
    {
        var funcionario = new Funcionario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Cargo = cargo,
            SenhaHash = senhaHash,
            Ativo = true
        };

        if (string.IsNullOrWhiteSpace(nome))
            funcionario.AddNotification(nameof(Nome), "Nome do funcionário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            funcionario.AddNotification(nameof(Email), "Email do funcionário é obrigatório.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            funcionario.AddNotification(nameof(SenhaHash), "Senha do funcionário é obrigatória.");

        return funcionario;
    }

    public void Desativar() => Ativo = false;
}
