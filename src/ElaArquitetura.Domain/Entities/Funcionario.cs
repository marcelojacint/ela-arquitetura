using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.ValueObjects;

namespace ElaArquitetura.Domain.Entities;

public class Funcionario : Notifiable
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Cargo { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public Telefone? Telefone { get; private set; }
    public bool Ativo { get; private set; }

    protected Funcionario()
    {
    }

    public static Funcionario Criar(string nome, string email, string cargo, string senhaHash, string? telefoneBruto = null)
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

        funcionario.DefinirTelefone(telefoneBruto);

        return funcionario;
    }

    public void Atualizar(string nome, string email, string cargo, string? telefoneBruto)
    {
        if (string.IsNullOrWhiteSpace(nome))
            AddNotification(nameof(Nome), "Nome do funcionário é obrigatório.");
        else
            Nome = nome;

        if (string.IsNullOrWhiteSpace(email))
            AddNotification(nameof(Email), "Email do funcionário é obrigatório.");
        else
            Email = email;

        if (string.IsNullOrWhiteSpace(cargo))
            AddNotification(nameof(Cargo), "Cargo do funcionário é obrigatório.");
        else
            Cargo = cargo;

        DefinirTelefone(telefoneBruto);
    }

    public void Desativar() => Ativo = false;

    private void DefinirTelefone(string? telefoneBruto)
    {
        if (string.IsNullOrWhiteSpace(telefoneBruto))
        {
            Telefone = null;
            return;
        }

        if (Telefone.TryCriar(telefoneBruto, out var telefone, out var erroTelefone))
            Telefone = telefone;
        else
            AddNotification(nameof(Telefone), erroTelefone!);
    }
}
