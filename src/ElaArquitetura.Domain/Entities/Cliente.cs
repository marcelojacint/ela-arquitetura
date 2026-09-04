using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.ValueObjects;

namespace ElaArquitetura.Domain.Entities;

public class Cliente : Notifiable
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Telefone? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Endereco { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool Ativo { get; private set; }

    protected Cliente()
    {
    }

    public static Cliente Criar(string nome, string telefoneBruto, string? email = null, string? endereco = null)
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Endereco = endereco,
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        if (string.IsNullOrWhiteSpace(nome))
            cliente.AddNotification(nameof(Nome), "Nome do cliente é obrigatório.");

        if (Telefone.TryCriar(telefoneBruto, out var telefone, out var erroTelefone))
            cliente.Telefone = telefone;
        else
            cliente.AddNotification(nameof(Telefone), erroTelefone!);

        return cliente;
    }

    public void Desativar() => Ativo = false;
}
