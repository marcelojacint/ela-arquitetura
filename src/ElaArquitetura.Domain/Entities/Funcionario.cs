using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.ValueObjects;

namespace ElaArquitetura.Domain.Entities;

/// <summary>
/// Pessoa que trabalha no escritório. O sistema tem uma única usuária (a dona do escritório),
/// que também é um Funcionario, mas com senha — é a única que faz login (<see cref="PossuiAcesso"/>).
/// Os demais funcionários são só um cadastro para consulta e não têm senha nem cargo.
/// </summary>
public class Funcionario : Notifiable
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Cargo { get; private set; }
    public string? SenhaHash { get; private set; }
    public Telefone? Telefone { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public bool Ativo { get; private set; }

    /// <summary>Só a usuária do sistema tem senha e, portanto, consegue fazer login.</summary>
    public bool PossuiAcesso => !string.IsNullOrEmpty(SenhaHash);

    protected Funcionario()
    {
    }

    /// <summary>Cria a usuária do sistema (com senha, para login).</summary>
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

        funcionario.ValidarNomeEEmail();

        if (string.IsNullOrWhiteSpace(senhaHash))
            funcionario.AddNotification(nameof(SenhaHash), "Senha do funcionário é obrigatória.");

        funcionario.DefinirTelefone(telefoneBruto);

        return funcionario;
    }

    /// <summary>Cadastra um funcionário do escritório, só para consulta (sem acesso ao sistema).</summary>
    public static Funcionario Cadastrar(string nome, string email, string? telefoneBruto, DateOnly? dataNascimento)
    {
        var funcionario = new Funcionario
        {
            Id = Guid.NewGuid(),
            Nome = nome?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            Ativo = true
        };

        funcionario.ValidarNomeEEmail();
        funcionario.DefinirTelefone(telefoneBruto);
        funcionario.DefinirDataNascimento(dataNascimento);

        return funcionario;
    }

    public void Atualizar(string nome, string email, string? telefoneBruto, DateOnly? dataNascimento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            AddNotification(nameof(Nome), "Nome do funcionário é obrigatório.");
        else
            Nome = nome.Trim();

        if (string.IsNullOrWhiteSpace(email))
            AddNotification(nameof(Email), "Email do funcionário é obrigatório.");
        else
            Email = email.Trim();

        DefinirTelefone(telefoneBruto);
        DefinirDataNascimento(dataNascimento);
    }

    public void Desativar() => Ativo = false;

    private void ValidarNomeEEmail()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            AddNotification(nameof(Nome), "Nome do funcionário é obrigatório.");

        if (string.IsNullOrWhiteSpace(Email))
            AddNotification(nameof(Email), "Email do funcionário é obrigatório.");
    }

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

    private void DefinirDataNascimento(DateOnly? dataNascimento)
    {
        if (dataNascimento is { } data && data > DateOnly.FromDateTime(DateTime.Today))
        {
            AddNotification(nameof(DataNascimento), "Data de nascimento não pode estar no futuro.");
            return;
        }

        DataNascimento = dataNascimento;
    }
}
